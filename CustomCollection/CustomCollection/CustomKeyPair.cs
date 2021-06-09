using System;

namespace CustomCollection.CustomCollection
{
    internal class CustomKeyPair<TId, TName> : IEquatable<CustomKeyPair<TId, TName>>
    {
        public TId Id { get; set; }
        public TName Name { get; set; }

        public CustomKeyPair(TId id, TName name)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            if (name == null)
                throw new ArgumentNullException("name");

            this.Id = id;
            this.Name = name;
        }

        public bool Equals(CustomKeyPair<TId, TName> other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Id.Equals(other.Id) && Name.Equals(other.Name);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return obj is CustomKeyPair<TId, TName> pair && Equals(pair);
        }

        public override string ToString()
        {
            return $"[{this.Id}, {this.Name}].";
        }

        public override int GetHashCode()
        {
            return new { this.Id, this.Name }.GetHashCode();
        }
    }
}
