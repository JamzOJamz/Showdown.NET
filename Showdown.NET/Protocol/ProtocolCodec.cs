using System.Text.Json;
using JetBrains.Annotations;
using Showdown.NET.Simulator;

namespace Showdown.NET.Protocol;

[PublicAPI]
public static class ProtocolCodec
{
    public static string EncodeStartCommand(string formatId)
    {
        var payload = new { formatid = formatId };
        var json = JsonSerializer.Serialize(payload);
        return $">start {json}";
    }

    public static string EncodeSetPlayerCommand(int player, string name)
        => EncodeSetPlayerCommandInternal(player, name, null);

    public static string EncodeSetPlayerCommand(int player, string name, PokemonSet[]? team = null)
        => EncodeSetPlayerCommandInternal(player, name, team);

    public static string EncodeSetPlayerCommand(int player, string name, string? team = null)
        => EncodeSetPlayerCommandInternal(player, name, team);

    private static string EncodeSetPlayerCommandInternal(int player, string name, object? team)
    {
        var payload = new Dictionary<string, object?>
        {
            ["name"] = name
        };

        if (team != null)
            payload["team"] = team;

        var json = JsonSerializer.Serialize(payload);
        return $">player p{player} {json}";
    }

    public static string EncodePlayerChoiceCommand(int player, params string[] args)
    {
        if (args.Length == 0)
            throw new ArgumentException("At least one argument is required", nameof(args));

        var command = string.Join(" ", args);
        return $">p{player} {command}";
    }

    public static ProtocolFrame? Parse(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return null;

        var lines = message.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var messageType = lines[0].Trim();

        return messageType switch
        {
            "update" => ParseUpdateFrame(lines),
            "sideupdate" => ParseSideUpdateFrame(lines),
            "end" => ParseEndFrame(lines),
            _ => null
        };
    }

    private static UpdateFrame ParseUpdateFrame(string[] lines)
    {
        var parts = new List<ProtocolElement>();
        var i = 1;

        while (i < lines.Length)
        {
            var element = ParseProtocolElementWithSplit(lines, ref i);
            if (element != null)
                parts.Add(element);
        }

        return new UpdateFrame(parts);
    }

    private static SideUpdateFrame? ParseSideUpdateFrame(string[] lines)
    {
        if (lines.Length <= 1)
            return null;

        var player = lines[1].Trim();
        var parts = new List<ProtocolElement>();
        var i = 2;

        while (i < lines.Length)
        {
            var element = ParseProtocolElementWithSplit(lines, ref i);
            if (element != null)
                parts.Add(element);
        }

        return new SideUpdateFrame(player, parts);
    }

    private static EndFrame? ParseEndFrame(string[] lines)
    {
        return new EndFrame(lines[1]);
    }

    private static ProtocolElement? ParseProtocolElementWithSplit(string[] lines, ref int index)
    {
        if (index >= lines.Length)
            return null;

        var currentLine = lines[index].Trim();

        // Check if this is a split command
        if (currentLine.StartsWith("|split|"))
        {
            var splitSegments = currentLine[1..].Split('|');
            if (splitSegments.Length < 2)
            {
                index++;
                return null;
            }

            var playerId = splitSegments[1];

            // We need to peek ahead to get the secret and public elements
            if (index + 2 >= lines.Length)
            {
                index++;
                return null;
            }

            var secretElement = ParseProtocolElement(lines[index + 1]);
            var publicElement = ParseProtocolElement(lines[index + 2]);

            // Skip ahead by 3 lines (split + secret + public)
            index += 3;

            if (secretElement != null && publicElement != null)
                return CreateSplitElement(playerId, secretElement, publicElement);

            return null;
        }

        // Regular protocol element
        var element = ParseProtocolElement(currentLine);
        index++;
        return element;
    }

    private static ProtocolElement CreateSplitElement(string playerId, ProtocolElement secret, ProtocolElement @public)
    {
        // Using reflection to create the generic SplitElement with the correct type
        var secretType = secret.GetType();
        var splitElementType = typeof(SplitElement<>).MakeGenericType(secretType);

        return (ProtocolElement)Activator.CreateInstance(splitElementType, playerId[1] - '0', secret, @public)!;
    }

    private static ProtocolElement? ParseProtocolElement(string line)
    {
        var trimmedLine = line.Trim();

        if (!trimmedLine.StartsWith('|'))
            return null;

        var segments = trimmedLine[1..].Split('|');

        if (segments.Length < 1)
            return null;

        var command = segments[0];
        ProtocolElement elem;
        var usedCount = 0;

        // set to false if the tags are consumed by parsing
        // and in some way end up contained in the final element object
        bool useTags = true;

        switch (command)
        {
            // Battle initialization
            case "player" when segments.Length > 4:
                elem = new PlayerDetailsElement(segments[1][1] - '0', segments[2], segments[3], segments[4]);
                usedCount = 4;
                break;
            case "teamsize" when segments.Length > 2:
                elem = TeamSizeElement.Parse(segments[1], segments[2]);
                usedCount = 2;
                break;
            case "gametype" when segments.Length > 1:
                elem = GameTypeElement.Parse(segments[1]);
                usedCount = 1;
                break;
            case "gen" when segments.Length > 1:
                elem = GenElement.Parse(segments[1]);
                usedCount = 1;
                break;
            case "tier" when segments.Length > 1:
                elem = new TierElement(segments[1]);
                usedCount = 1;
                break;
            case "rated":
                elem = RatedElement.Parse(segments, out usedCount);
                break;
            case "rule":
                elem = RuleElement.Parse(segments);
                break;
            case "clearpoke":
                elem = new ClearPokeElement();
                break;
            case "poke" when segments.Length > 3:
                elem = new PokeElement(segments[1][1] - '0', segments[2], segments[3]);
                usedCount = 3;
                break;
            case "teampreview":
                elem = new TeamPreviewElement();
                break;
            case "start":
                elem = new StartElement();
                break;

            // Battle progress
            case "":
                elem = new SpacerElement();
                break;
            case "request" when segments.Length > 1:
                elem = new RequestElement(segments[1]);
                usedCount = 1;
                break;
            case "inactive" when segments.Length > 1:
                elem = new InactiveElement(segments[1]);
                usedCount = 1;
                break;
            case "inactiveoff" when segments.Length > 1:
                elem = new InactiveOffElement(segments[1]);
                usedCount = 1;
                break;
            case "upkeep":
                elem = new UpkeepElement();
                break;
            case "turn" when segments.Length > 1:
                elem = TurnElement.Parse(segments[1]);
                usedCount = 1;
                break;
            case "win" when segments.Length > 1:
                elem = new WinElement(segments[1]);
                usedCount = 1;
                break;
            case "tie":
                elem = new TieElement();
                break;
            case "t:" when segments.Length > 1:
                elem = TimestampElement.Parse(segments[1]);
                usedCount = 1;
                break;

            // Major actions
            case "move" when segments.Length > 3:
                elem = MoveElement.Parse(segments);
                useTags = false;
                break;
            case "switch" when segments.Length > 3:
                var (hp, status) = SwitchElement.ParseHPStatus(segments[3]);
                elem = new SwitchElement(segments[1], segments[2], hp, status);
                usedCount = 3;
                break;
            case "drag" when segments.Length > 3:
                (hp, status) = SwitchElement.ParseHPStatus(segments[3]);
                elem = new DragElement(segments[1], segments[2], hp, status);
                usedCount = 3;
                break;
            case "detailschange" when segments.Length > 3:
                (hp, status) = SwitchElement.ParseHPStatus(segments[3]);
                elem = new DetailsChangeElement(segments[1], segments[2], hp, status);
                usedCount = 3;
                break;
            case "-formechange" when segments.Length > 3:
                (hp, status) = SwitchElement.ParseHPStatus(segments[3]);
                elem = new FormeChangeElement(segments[1], segments[2], hp, status);
                usedCount = 3;
                break;
            case "replace" when segments.Length > 3:
                (hp, status) = SwitchElement.ParseHPStatus(segments[3]);
                elem = new ReplaceElement(segments[1], segments[2], hp, status);
                usedCount = 3;
                break;
            case "swap" when segments.Length > 2:
                elem = SwapElement.Parse(segments);
                usedCount = 2;
                break;
            case "cant" when segments.Length > 2:
                elem = CantElement.Parse(segments, out usedCount);
                break;
            case "faint" when segments.Length > 1:
                elem = new FaintElement(segments[1]);
                usedCount = 1;
                break;

            // Minor actions
            case "-fail" when segments.Length > 2:
                elem = new FailElement(segments[1], segments[2]);
                usedCount = 2;
                break;
            case "-block" when segments.Length > 2:
                elem = BlockElement.Parse(segments, out usedCount);
                break;
            case "-notarget":
                elem = NoTargetElement.Parse(segments, out usedCount);
                break;
            case "-miss" when segments.Length > 1:
                elem = MissElement.Parse(segments, out usedCount);
                break;
            case "-damage" when segments.Length > 2:
                elem = DamageElement.Parse(segments[1], segments[2], heal: false);
                usedCount = 2;
                break;
            case "-heal" when segments.Length > 2:
                elem = DamageElement.Parse(segments[1], segments[2], heal: true);
                usedCount = 2;
                break;
            case "-sethp" when segments.Length > 2:
                elem = new SetHPElement(segments[1], segments[2]);
                usedCount = 2;
                break;
            case "-status" when segments.Length > 2:
                elem = StatusElement.Parse(segments, cure: false);
                usedCount = 2;
                break;
            case "-curestatus" when segments.Length > 2:
                elem = StatusElement.Parse(segments, cure: true);
                usedCount = 2;
                break;
            case "-cureteam" when segments.Length > 1:
                elem = new CureTeamElement(segments[1]);
                usedCount = 1;
                break;
            case "-boost" when segments.Length > 3:
                elem = BoostElement.Parse(segments, boost: true);
                usedCount = 3;
                break;
            case "-unboost" when segments.Length > 3:
                elem = BoostElement.Parse(segments, boost: false);
                usedCount = 3;
                break;
            case "-setboost" when segments.Length > 3:
                elem = BoostElement.Parse(segments, boost: null);
                usedCount = 3;
                break;
            case "-swapboost" when segments.Length > 3:
                elem = SwapBoostElement.Parse(segments);
                usedCount = 3;
                break;
            case "-invertboost" when segments.Length > 1:
                elem = new InvertBoostElement(segments[1]);
                usedCount = 1;
                break;
            case "-clearboost" when segments.Length > 1:
                elem = new ClearBoostElement(segments[1]);
                usedCount = 1;
                break;
            case "-clearallboost":
                elem = new ClearAllBoostElement();
                break;
            case "-clearpositiveboost" when segments.Length > 3:
                elem = new ClearPositiveBoostElement(segments[1], segments[2], segments[3]);
                usedCount = 3;
                break;
            case "-clearnegativeboost" when segments.Length > 1:
                elem = new ClearNegativeBoostElement(segments[1]);
                usedCount = 1;
                break;
            case "-copyboost" when segments.Length > 2:
                elem = new CopyBoostElement(segments[1], segments[2]);
                usedCount = 2;
                break;
            case "-weather" when segments.Length > 1:
                elem = WeatherElement.Parse(segments);
                useTags = false;
                break;
            case "-fieldstart" when segments.Length > 1:
                elem = new FieldStartElement(segments[1]);
                usedCount = 1;
                break;
            case "-fieldend" when segments.Length > 1:
                elem = new FieldEndElement(segments[1]);
                usedCount = 1;
                break;
            case "-sidestart" when segments.Length > 2:
                elem = SideStartElement.Parse(segments);
                usedCount = 2;
                break;
            case "-sideend" when segments.Length > 2:
                elem = SideEndElement.Parse(segments);
                usedCount = 2;
                break;
            case "-swapsideconditions":
                elem = new SwapSideConditionsElement();
                break;
            case "-start" when segments.Length > 2:
                elem = new StartVolatileElement(segments[1], segments[2]);
                usedCount = 2;
                break;
            case "-end" when segments.Length > 2:
                elem = new EndVolatileElement(segments[1], segments[2]);
                usedCount = 2;
                break;
            case "-crit" when segments.Length > 1:
                elem = new CritElement(segments[1]);
                usedCount = 1;
                break;
            case "-supereffective" when segments.Length > 1:
                elem = new SuperEffectiveElement(segments[1]);
                usedCount = 1;
                break;
            case "-resisted" when segments.Length > 1:
                elem = new ResistedElement(segments[1]);
                usedCount = 1;
                break;
            case "-immune" when segments.Length > 1:
                elem = new ImmuneElement(segments[1]);
                usedCount = 1;
                break;
            case "-item" when segments.Length > 2:
                elem = new ItemElement(segments[1], segments[2]);
                usedCount = 2;
                break;
            case "-enditem" when segments.Length > 2:
                elem = new EndItemElement(segments[1], segments[2]);
                usedCount = 2;
                break;
            case "-ability" when segments.Length > 2:
                elem = new AbilityElement(segments[1], segments[2]);
                usedCount = 2;
                break;
            case "-endability" when segments.Length > 1:
                elem = new EndAbilityElement(segments[1]);
                usedCount = 1;
                break;
            case "-transform" when segments.Length > 2:
                elem = new TransformElement(segments[1], segments[2]);
                usedCount = 2;
                break;
            case "-mega" when segments.Length > 2:
                elem = new MegaElement(segments[1], segments[2]);
                usedCount = 2;
                break;
            case "-primal" when segments.Length > 1:
                elem = new PrimalElement(segments[1]);
                usedCount = 1;
                break;
            case "-burst" when segments.Length > 3:
                elem = new BurstElement(segments[1], segments[2], segments[3]);
                usedCount = 3;
                break;
            case "-zpower" when segments.Length > 1:
                elem = new ZPowerElement(segments[1]);
                usedCount = 1;
                break;
            case "-zbroken" when segments.Length > 1:
                elem = new ZBrokenElement(segments[1]);
                usedCount = 1;
                break;
            case "-activate" when segments.Length > 2:
                elem = new ActivateElement(segments[1], segments[2]);
                usedCount = 2;
                break;
            case "-hint" when segments.Length > 1:
                elem = new HintElement(segments[1]);
                usedCount = 1;
                break;
            case "-center":
                elem = new CenterElement();
                break;
            case "-message" when segments.Length > 1:
                elem = new MessageElement(segments[1]);
                usedCount = 1;
                break;
            case "-combine":
                elem = new CombineElement();
                usedCount = 1;
                break;
            case "-waiting" when segments.Length > 2:
                elem = new WaitingElement(segments[1], segments[2]);
                usedCount = 2;
                break;
            case "-prepare" when segments.Length > 2:
                elem = PrepareElement.Parse(segments, out usedCount);
                break;
            case "-mustrecharge" when segments.Length > 1:
                elem = new MustRechargeElement(segments[1]);
                usedCount = 1;
                break;
            case "-nothing":
                elem = new NothingElement();
                break;
            case "-hitcount" when segments.Length > 2:
                elem = HitCountElement.Parse(segments[1], segments[2]);
                usedCount = 2;
                break;
            case "-singlemove" when segments.Length > 2:
                elem = new SingleMoveElement(segments[1], segments[2]);
                usedCount = 2;
                break;
            case "-singleturn" when segments.Length > 2:
                elem = new SingleTurnElement(segments[1], segments[2]);
                usedCount = 2;
                break;

            // Miscellaneous
            case "debug" when segments.Length > 1:
                elem = new DebugElement(segments[1]);
                usedCount = 1;
                break;
            case "error" when segments.Length > 1:
                elem = ErrorElement.Parse(segments[1]);
                useTags = false;
                break;

            // Fallback for unprocessable elements
            default:
                elem = new UnknownElement(trimmedLine);
                useTags = false;
                break;
        }

        // Store all remaining segments as tags
        if (useTags && segments.Length > usedCount + 1)
            elem.Tags = segments[(usedCount + 1)..];

        return elem;
    }
}
