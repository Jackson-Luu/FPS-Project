namespace Mirror
{
    public class RoomPlayer : NetworkRoomPlayer
    {
        NetworkRoomManager nrm;

        [SyncVar(hook = nameof(SyncCountdown))]
        public int countingDown;

        private bool firstSetup = true;

        void SyncCountdown(int _, int countDown)
        {
            (NetworkManager.singleton as NetworkRoomManager).countdown = countDown;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            nrm = NetworkManager.singleton as NetworkRoomManager;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            countingDown = (NetworkManager.singleton as NetworkRoomManager).countdown;
        }

        public override void OnClientEnterRoom()
        {
            if (nrm == null) { return; }
            if (firstSetup)
            {
                nrm.roomPlayers++;
                firstSetup = false;
            } else
            {
                // No need to register, returning to room player
                return;
            }
            if (nrm.onRoomStatusChanged != null)
            {
                nrm.onRoomStatusChanged.Invoke(false);
            }
        }

        public override void OnClientReady(bool readyState)
        {
            if (readyState)
            {
                nrm.playersReady++;
            } else
            {
                nrm.playersReady--;
            }
            if (nrm.onRoomStatusChanged != null)
            {
                nrm.onRoomStatusChanged.Invoke(false);
            }
        }

        [TargetRpc]
        public override void TargetBeginRoyaleCountdown(NetworkConnection conn)
        {
            if (nrm.onRoomStatusChanged != null)
            {
                nrm.onRoomStatusChanged.Invoke(true);
            }
        }
    }
}
