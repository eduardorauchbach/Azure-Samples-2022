using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace RauchTech.Extensions.Data.Cosmos.Models
{
    public abstract class EntityRoot : IEquatable<EntityRoot>
    {
        #region Properties
        /// <summary>
        /// Default id that any cosmos collection has
        /// </summary>
        [JsonPropertyName("id")]
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid Id { get; set; }
        #endregion

        ///// <summary>
        ///// Constructor for base class. Generate a new default GUID when empty.
        ///// </summary>
        //protected EntityRoot()
        //{
        //    Id = Id.Equals(Guid.Empty) ? Guid.NewGuid() : Id;
        //}

        /// <summary>
        /// Implements equal operator
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True when objects are equals. Otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            EntityRoot? compareTo = obj as EntityRoot;

            if (ReferenceEquals(this, compareTo))
            {
                return true;
            }

            return compareTo != null && Id.Equals(compareTo.Id);
        }

        /// <summary>
        /// Calculare HashCode from object
        /// </summary>
        /// <returns>HashCode's Number</returns>
        public override int GetHashCode()
        {
            return (GetType().GetHashCode() * 907) + Id.GetHashCode();
        }

        /// <summary>
        /// ToString representation, which shows the Entity's ID and type's name.
        /// </summary>
        /// <returns>Entity's ID and type's name</returns>
        public override string ToString()
        {
            return GetType().Name + " [Id=" + Id + "]";
        }

        /// <summary>
        /// Compare one entity with another
        /// </summary>
        /// <param name="other">Other entity to compare</param>
        /// <returns>True whem entities are equal. Otherwise, false.</returns>
        public bool Equals(EntityRoot? other)
        {
            return Equals(other);
        }
    }
}
