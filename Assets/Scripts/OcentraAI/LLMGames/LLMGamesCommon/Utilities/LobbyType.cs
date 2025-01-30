using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using Object = UnityEngine.Object;


namespace OcentraAI.LLMGames.GameModes
{
    [Serializable]
    public class LobbyType : IEquatable<LobbyType>, ILabeledItem
    {
        #region Static Instances

        public static readonly LobbyType None = new LobbyType(
            id: 0,
            name: nameof(None),
            hostingMethod: HostingMethodType.None,
            llmSource: LLMSourceType.None,
            requiresDesktop: false,
            isPlayerCreatable: false,
            supportsWeb: false,
            supportsVR: false,
            supportsAR: false
        );

        public static readonly LobbyType DedicatedServer = new LobbyType(
            id: 1,
            name: nameof(DedicatedServer),
            hostingMethod: HostingMethodType.DedicatedServer,
            llmSource: LLMSourceType.CloudService,
            requiresDesktop: false,
            isPlayerCreatable: false,
            supportsWeb: true,
            supportsVR: false,
            supportsAR: false
        );

        public static readonly LobbyType PlayerLocalLLM = new LobbyType(
            id: 2,
            name: nameof(PlayerLocalLLM),
            hostingMethod: HostingMethodType.PlayerHosted,
            llmSource: LLMSourceType.LocalEmbedded,
            requiresDesktop: true,
            isPlayerCreatable: true,
            supportsWeb: false,
            supportsVR: true,
            supportsAR: false
        );

        public static readonly LobbyType PlayerLocalAPI = new LobbyType(
            id: 3,
            name: nameof(PlayerLocalAPI),
            hostingMethod: HostingMethodType.PlayerHosted,
            llmSource: LLMSourceType.LocalAPI,
            requiresDesktop: true,
            isPlayerCreatable: true,
            supportsWeb: false,
            supportsVR: false,
            supportsAR: true
        );

        public static readonly LobbyType PlayerRemoteAPI = new LobbyType(
            id: 4,
            name: nameof(PlayerRemoteAPI),
            hostingMethod: HostingMethodType.PlayerHosted,
            llmSource: LLMSourceType.RemoteAPI,
            requiresDesktop: false,
            isPlayerCreatable: true,
            supportsWeb: true,
            supportsVR: true,
            supportsAR: true
        );

        #endregion

        #region Cached Data

        private static LobbyType[] cachedAll;
        private static Dictionary<int, LobbyType> idLookup;
        private static bool isLookupBuilt;

        #endregion

        #region Instance Properties

        [ShowInInspector, ReadOnly] private int id;
        [ShowInInspector, ReadOnly] private string name;
        [ShowInInspector, ReadOnly] private HostingMethodType hostingMethodType;
        [ShowInInspector, ReadOnly] private LLMSourceType llmSourceType;
        [ShowInInspector, ReadOnly] private bool requiresDesktop;
        [ShowInInspector, ReadOnly] private bool isPlayerCreatable;
        [ShowInInspector, ReadOnly] private bool supportsWeb;
        [ShowInInspector, ReadOnly] private bool supportsVR;
        [ShowInInspector, ReadOnly] private bool supportsAR;

        #endregion

        #region Constructor

        private LobbyType(int id, string name,
            HostingMethodType hostingMethod, LLMSourceType llmSource,
            bool requiresDesktop, bool isPlayerCreatable, bool supportsWeb,
            bool supportsVR = false, bool supportsAR = false)
        {
            this.id = id;
            this.name = name;
            hostingMethodType = hostingMethod;
            llmSourceType = llmSource;
            this.requiresDesktop = requiresDesktop;
            this.isPlayerCreatable = isPlayerCreatable;
            this.supportsWeb = supportsWeb;
            this.supportsVR = supportsVR;
            this.supportsAR = supportsAR;
        }

        #endregion

        #region Public Properties

        public int Id => id;
        public string Name => name;
        public HostingMethodType HostingMethod => hostingMethodType;
        public LLMSourceType LLMSource => llmSourceType;
        public bool RequiresDesktop => requiresDesktop;
        public bool IsPlayerCreatable => isPlayerCreatable;
        public bool SupportsWeb => supportsWeb;
        public bool SupportsVR => supportsVR;
        public bool SupportsAR => supportsAR;
        public bool SupportsDesktop => SupportsWeb || RequiresDesktop;
        public bool SupportsMobile => SupportsWeb && !RequiresDesktop;
        public bool SupportsConsole => SupportsWeb;
        public bool SupportsVRAndAR => SupportsVR || SupportsAR;

        #endregion

        #region Core Methods

        public static LobbyType[] GetAll()
        {
            if (cachedAll != null) return cachedAll;

            FieldInfo[] fields = typeof(LobbyType).GetFields(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.DeclaredOnly
            );

            List<LobbyType> results = new List<LobbyType>(fields.Length);

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (field.FieldType == typeof(LobbyType))
                {
                    LobbyType lobby = (LobbyType)field.GetValue(null);
                    results.Add(lobby);
                }
            }

            cachedAll = results.ToArray();
            return cachedAll;
        }

        public static LobbyType FromId(int id)
        {
            if (!isLookupBuilt)
            {
                LobbyType[] all = GetAll();
                idLookup = new Dictionary<int, LobbyType>(all.Length);
                for (int i = 0; i < all.Length; i++)
                {
                    idLookup[all[i].Id] = all[i];
                }

                isLookupBuilt = true;
            }

            LobbyType result;
            if (idLookup.TryGetValue(id, out result))
            {
                return result;
            }

            Debug.LogWarning($"LobbyType with ID {id} not found");
            return None;
        }

        #endregion

        #region Platform Methods

        public static IEnumerable<LobbyType> GetAvailableForDevice(ExtendedDeviceType deviceType)
        {
            LobbyType[] all = GetAll();
            foreach (LobbyType lobby in all)
            {
                bool supported = deviceType switch
                {
                    ExtendedDeviceType.Desktop => lobby.SupportsDesktop,
                    ExtendedDeviceType.Handheld => lobby.SupportsMobile,
                    ExtendedDeviceType.Console => lobby.SupportsConsole,
                    ExtendedDeviceType.Web => lobby.SupportsWeb,
                    ExtendedDeviceType.VR => lobby.SupportsVR,
                    ExtendedDeviceType.AR => lobby.SupportsAR,
                    _ => false
                };

                if (supported)
                {
                    yield return lobby;
                }
            }
        }

        public static IEnumerable<LobbyType> GetAvailableForCurrentDevice()
        {
            return GetAvailableForDevice(DeviceTypeDetector.GetDeviceType());
        }

        public bool IsSupportedOnDevice(ExtendedDeviceType deviceType)
        {
            LobbyType[] available = GetAll();
            for (int i = 0; i < available.Length; i++)
            {
                if (available[i].Id == this.Id)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Validation

        public bool ValidateForDevice(ExtendedDeviceType deviceType, out string errorMessage)
        {
            errorMessage = null;

            if (this == None)
            {
                errorMessage = "No lobby type selected";
                return false;
            }

            if (LLMSource == LLMSourceType.LocalEmbedded &&
                HostingMethod != HostingMethodType.PlayerHosted)
            {
                errorMessage = "Local embedded LLM requires player-hosted lobby";
                return false;
            }

            if (!IsSupportedOnDevice(deviceType))
            {
                errorMessage = $"{Name} not supported on {deviceType} devices";
                return false;
            }

            if (IsPlayerCreatable && RequiresDesktop && deviceType != ExtendedDeviceType.Desktop)
            {
                errorMessage = $"{Name} requires desktop environment for hosting";
                return false;
            }

            return true;
        }

        #endregion

        #region Networking & Conversions

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref id);
        }

        public static implicit operator int(LobbyType lobbyType) => lobbyType?.Id ?? 0;
        public static explicit operator LobbyType(int id) => FromId(id);

        #endregion

        #region Equality & Debug

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is LobbyType other && Equals(other);
        }

        public bool Equals(LobbyType other) => !(other is null) && Id == other.Id;

        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(LobbyType left, LobbyType right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(LobbyType left, LobbyType right) => !(left == right);

        public override string ToString() => $"{Name} [ID: {Id}]";

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogPerformanceWarning(string message)
        {
            Debug.LogWarning($"LobbyType Performance: {message}");
        }

        #endregion
    }

    public enum ExtendedDeviceType
    {
        Desktop,
        Handheld,
        Console,
        Web,
        VR,
        AR
    }

    public static class DeviceTypeDetector
    {
        public static ExtendedDeviceType GetDeviceType()
        {
            if (IsVRDevice())
            {
                return ExtendedDeviceType.VR;
            }

            if (IsARDevice())
            {
                return ExtendedDeviceType.AR;
            }

            return SystemInfo.deviceType switch
            {
                DeviceType.Desktop => ExtendedDeviceType.Desktop,
                DeviceType.Handheld => ExtendedDeviceType.Handheld,
                DeviceType.Console => ExtendedDeviceType.Console,
                _ => ExtendedDeviceType.Web
            };
        }

        private static bool IsVRDevice()
        {
            return XRSettings.isDeviceActive && !string.IsNullOrEmpty(XRSettings.loadedDeviceName);
        }

        private static bool IsARDevice()
        {
            ARSession arSession = Object.FindFirstObjectByType<ARSession>();
            if (arSession == null)
            {
                return false;
            }

            return ARSession.state == ARSessionState.SessionTracking;
        }

    }
}

