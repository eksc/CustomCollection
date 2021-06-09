using System;

namespace CustomCollection.Models
{
    internal class UserType : IEquatable<UserType>
    {
        public Guid Guid { get; set; }
        public string FullName { get; set; }
        public DateTime BirthDay { get; set; }

        public UserType() { }

        public UserType(Guid guid, string fullName, DateTime birthDay)
        {
            this.Guid = guid;
            this.FullName = fullName;
            this.BirthDay = birthDay;
        }

        public bool Equals(UserType other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return this.Guid.Equals(other.Guid) && 
                   this.FullName.Equals(other.FullName) && 
                   this.BirthDay.Equals(other.BirthDay);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return Equals((UserType)obj);
        }

        public override string ToString()
        {
            return $"User {this.FullName} with birthday {this.BirthDay.ToShortDateString()}.";
        }

        public override int GetHashCode()
        {
            return new 
                   { 
                       this.Guid,
                       this.FullName,
                       this.BirthDay 
                   }.GetHashCode();
        }
    }
}
