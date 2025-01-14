using System.Collections.Generic;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class AuthStatus: IAuthStatus
    {
        public static readonly AuthStatus Success = new AuthStatus(0, nameof(Success));
        public static readonly AuthStatus Failure = new AuthStatus(1, nameof(Failure));
        public static readonly AuthStatus Pending = new AuthStatus(2, nameof(Pending));
        public static readonly AuthStatus Authenticated = new AuthStatus(3, nameof(Authenticated));

        private static readonly List<AuthStatus> AllStatuses = new List<AuthStatus>
        {
            Success,
            Failure,
            Pending,
            Authenticated
        };

        public int StatusCode { get; }
        public string Name { get; }

        private AuthStatus(int statusCode, string name)
        {
            StatusCode = statusCode;
            Name = name;
        }

        public static AuthStatus FromStatusCode(int statusCode)
        {
            foreach (var status in AllStatuses)
            {
                if (status.StatusCode == statusCode)
                {
                    return status;
                }
            }

            throw new ArgumentException($"Invalid status code: {statusCode}");
        }

        public static AuthStatus FromName(string name)
        {
            foreach (var status in AllStatuses)
            {
                if (string.Equals(status.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return status;
                }
            }

            throw new ArgumentException($"Invalid status name: {name}");
        }

        public override bool Equals(object obj)
        {
            if (obj is AuthStatus other)
            {
                return StatusCode == other.StatusCode && Name == other.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StatusCode, Name);
        }

        public override string ToString()
        {
            return Name;
        }

        public static IEnumerable<AuthStatus> GetAllStatuses()
        {
            return AllStatuses.AsReadOnly();
        }
    }

}