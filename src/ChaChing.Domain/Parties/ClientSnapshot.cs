using ChaChing.Domain.Common;

namespace ChaChing.Domain.Parties
{
    /// <summary>
    /// The client's details frozen on the day a document is sent (ADR 0012).
    /// Built only from a <see cref="Client"/>.
    /// </summary>
    public sealed record ClientSnapshot
    {
        private ClientSnapshot(Guid clientId, string name, Address address, string? siren)
        {
            ClientId = clientId;
            Name = name;
            Address = address;
            Siren = siren;
        }

        public static ClientSnapshot From(Client client)
        {
            ArgumentNullException.ThrowIfNull(client);
            return new ClientSnapshot(client.Id, client.Name, client.Address, client.Siren);
        }

        /// <summary>Which client the snapshot was taken from, so the quote can check it is its own.</summary>
        public Guid ClientId { get; }

        public string Name { get; }

        public Address Address { get; }

        public string? Siren { get; }
    }
}
