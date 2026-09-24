namespace ChaChing.Clients
{
    public class Client
    {
        public Guid Id { get; }

        public string Nom { get; }

        public string Email { get; private set; }

        private Client(Guid id, string nom, string email)
        {
            if (string.IsNullOrWhiteSpace(nom))
                throw new ArgumentException("Le nom est obligatoire.", nameof(nom));

            Id = id;
            Nom = nom.Trim();
            Email = ValiderEmail(email);
        }

        public static Client Creer(string nom, string email) => new Client(Guid.NewGuid(), nom, email);

        public static Client Reconstituer(Guid id, string nom, string email)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("L'id est vide", nameof(id));

            return new Client(id, nom, email);
        }

        public void ChangerEmail(string email)
        {
            Email = ValiderEmail(email);
        }

        private static string ValiderEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("L'email est obligatoire.", nameof(email));

            if (!email.Contains('@'))
                throw new ArgumentException("L'email doit contenir un @.", nameof(email));

            return email.Trim();
        }

        public override bool Equals(object? obj) => obj is Client autre && autre.Id == Id;

        public override int GetHashCode() => Id.GetHashCode();

    }
}
