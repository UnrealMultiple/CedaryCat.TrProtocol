using RealmNexus.Logging;
using TrProtocol;
using TrProtocol.Models.Interfaces;
using TrProtocol.NetPackets;

namespace RealmNexus.Core.Handlers;

public class SSCHandler(RealmClient client, ILogger logger, SyncPlayerHandler syncPlayerHandler) 
    : PacketHandlerBase(client, logger)
{
    private enum State
    {
        FreshConnection,
        SSC,
        NonSSC
    }

    private State _current = State.FreshConnection;
    private SyncPlayer _syncPlayer;
    private PlayerMana _playerMana;
    private PlayerHealth _playerHealth;
    private AnglerQuestCountSync _anglerQuest;
    private byte _currentSlot;
    private bool _newServer;
    private readonly Dictionary<short, SyncEquipment> _equipments = [];
    private readonly SyncPlayerHandler _syncPlayerHandler = syncPlayerHandler;

    public override void OnC2S(PacketInterceptArgs args)
    {
        if (_current == State.SSC) return;

        switch (args.Packet)
        {
            case SyncEquipment equip:
                _equipments[equip.ItemSlot] = equip;
                break;
            case SyncPlayer plr:
                _syncPlayer = plr;
                break;
            case PlayerMana mana:
                _playerMana = mana;
                break;
            case PlayerHealth health:
                _playerHealth = health;
                break;
            case AnglerQuestCountSync angler:
                _anglerQuest = angler;
                break;
        }
    }

    public override void OnS2C(PacketInterceptArgs args)
    {
        switch (args.Packet)
        {
            case LoadPlayer plr:
                _currentSlot = plr.PlayerSlot;
                _newServer = true;
                break;
            case WorldData data:
                var isSSC = data.EventInfo1[6];

                if (_current == State.SSC && !isSSC)
                    _ = RestoreCharacterAsync();

                if (isSSC && _newServer)
                {
                    _ = Client.SendPacketToClientAsync(new AddPlayerBuff
                    {
                        OtherPlayerSlot = _syncPlayerHandler.PlayerSlot,
                        BuffType = 156, // stoned
                        BuffTime = 300
                    });
                    _ = Client.SendPacketToClientAsync(new AddPlayerBuff
                    {
                        OtherPlayerSlot = _syncPlayerHandler.PlayerSlot,
                        BuffType = 149, // webbed
                        BuffTime = 300
                    });
                    _newServer = false;
                }

                _current = isSSC ? State.SSC : State.NonSSC;
                break;
            case StartPlaying:
                if (_current == State.SSC)
                {
                    _ = Client.SendPacketToClientAsync(new PlayerBuffs
                    {
                        PlayerSlot = _syncPlayerHandler.PlayerSlot,
                        BuffTypes = new ushort[44]
                    });
                }
                break;
        }
    }

    private IEnumerable<IPlayerSlot> GetRestores()
    {
        foreach (var equip in _equipments.Values)
            yield return equip;
        yield return _syncPlayer;
        yield return _playerMana;
        yield return _playerHealth;
        yield return _anglerQuest;
    }

    private async Task RestoreCharacterAsync()
    {
        foreach (var restore in GetRestores())
        {
            restore.PlayerSlot = _currentSlot;
            await Client.SendPacketToClientAsync((INetPacket)restore);
        }
    }

    public override void OnServerChanging()
    {
        _current = State.FreshConnection;
        _equipments.Clear();
        _newServer = false;
    }
}
