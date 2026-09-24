namespace ChaChing.Domain.Clients
{
    public class Client
    {
        public Guid Id { get; }

        public string Name { get; }

        public string Email { get; private set; }

        private Client(Guid id, string name, string email)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The name is required.", nameof(name));

            Id = id;
            Name = name.Trim();
            Email = ValidateEmail(email);
        }

        public static Client Create(string name, string email) => new Client(Guid.NewGuid(), name, email);

        public static Client Rehydrate(Guid id, string name, string email)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("The id is empty.", nameof(id));

            return new Client(id, name, email);
        }

        public void ChangeEmail(string email)
        {
            Email = ValidateEmail(email);
        }

        private static string ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("The email is required.", nameof(email));

            if (!email.Contains('@'))
                throw new ArgumentException("The email must contain an @.", nameof(email));

            return email.Trim();
        }

        public override bool Equals(object? obj) => obj is Client other && other.Id == Id;

        public override int GetHashCode() => Id.GetHashCode();

    }
}
