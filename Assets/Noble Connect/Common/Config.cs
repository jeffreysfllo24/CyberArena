using NobleConnect.Ice;
using System;
using System.Text;
using UnityEngine;

namespace NobleConnect
{
    [Serializable]
    public class Config
    {
        [NonSerialized]
        public ushort IcePort;
        [NonSerialized]
        public string Username;
        [NonSerialized]
        public string Password;
        [NonSerialized]
        public string Origin;
        [NonSerialized]
        public bool UseSimpleAddressGathering = false;

        public Action<string> OnFatalError;
        public Action OnOfferFailed;

        /// <summary>The geographic region to use when selecting a relay server.</summary>
        /// <remarks>
        /// Defaults to AUTO which will automatically select the closest region.
        /// This is useful if you would like your players to be able to choose
        /// their region at run time.
        /// Note that players are not prevented from connecting across regions.
        /// That would need to be implementing separately via matchmaking for
        /// example, by filtering out matches from undesired regions.
        /// </remarks>
        [Tooltip("The geographic region to use when selecting a relay server.")]
        public GeographicRegion Region;

        /// <summary>You can enable this to force relay connections to be used for testing purposes.</summary>
        /// <remarks>
        /// Disables punchthrough and direct connections. Forces connections to use the relays.
        /// This is useful if you want to test your game with the unavoidable latency that is 
        /// introduced when the relay servers are used.
        /// </remarks>
        [Tooltip("Enable this to force relay connections to be used for testing purposes.")]
        public bool ForceRelayOnly = false;

        /// <summary>Request timeout.</summary>
        /// <remarks>
        /// This effects how long to wait before considering a request to have failed.
        /// Requests are used during the punchthrough process and for setting up and maintaining relays.
        /// If you are allowing cross-region play or expect high latency you can increase this so that requests won't time out.
        /// The drawback is that waiting longer for timeouts causes it take take longer to detect actual failed requests so the
        /// connection process may take longer.
        /// </remarks>
        [Tooltip("How long to wait before considering a request to have failed.")]
        public float RequestTimeout = .2f;

        /// <summary>Initial timeout before resending refresh messages. This is doubled for each failed resend.</summary>
        [Tooltip("Initial timeout before resending refresh messages. This is doubled for each failed resend.")]
        public float RelayRequestTimeout = .1f;

        /// <summary>Max number of times to try and resend refresh messages before giving up and shutting down the relay connection.</summary>
        [Tooltip("Max number of times to try and resend refresh messages before giving up and shutting down the relay connection.")]
        public int RelayRefreshMaxAttempts = 8;

        /// <summary>How long a relay will stay alive without being refreshed</summary>
        /// <remarks>
        /// Setting this value higher means relays will stay alive longer even if the host temporarily loses connection or otherwise fails to send the refresh request in time.
        /// This can be helpful to maintain connection on an undependable network or when heavy application load (such as loading large levels synchronously) temporarily prevents requests from being processed.
        /// The drawback is that CCU is used for as long as the relay stays alive, so players that crash or otherwise don't clean up properly can cause lingering CCU usage for up to relayLifetime seconds.
        /// </remarks>
        [Tooltip("How long a relay will stay alive without being refreshed.")]
        public int RelayLifetime = 60;

        /// <summary>How often to send relay refresh requests.</summary>
        [Tooltip("How often to send relay refresh requests.")]
        public int RelayRefreshTime = 30;

        public IceConfig AsIceConfig()
        {
            // Get a reference to the NobleConnectSettings
            var settings = (NobleConnectSettings)Resources.Load("NobleConnectSettings", typeof(NobleConnectSettings));

            // Parse the username, password, and origin from the game id
            string username = "", password = "", origin = "";
            if (!string.IsNullOrEmpty(settings.gameID))
            {
                string decodedGameID = Encoding.UTF8.GetString(Convert.FromBase64String(settings.gameID));
                string[] parts = decodedGameID.Split('\n');

                if (parts.Length == 3)
                {
                    username = parts[1];
                    password = parts[2];
                    origin = parts[0];
                }
            }

            var iceConfig = new IceConfig {
                iceServerAddress = RegionURL.FromRegion(Region),
                icePort = settings.relayServerPort,
                username = username,
                password = password,
                origin = origin,
                useSimpleAddressGathering = (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android) && !Application.isEditor,
                onFatalError = OnFatalError,
                onOfferFailed = () => OnFatalError("Offer failed"),
                forceRelayOnly = ForceRelayOnly,
                RequestTimeout = RequestTimeout,
                RelayRequestTimeout = RelayRequestTimeout,
                RelayRefreshMaxAttempts = RelayRefreshMaxAttempts,
                RelayLifetime = RelayLifetime,
                RelayRefreshTime = RelayRefreshTime
            };

            return iceConfig;
        }
    }
}