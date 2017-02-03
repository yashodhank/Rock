using System.ComponentModel.DataAnnotations;
using Rock.Data;

namespace Rock.BulkUpdate
{
    /// <summary>
    /// 
    /// </summary>
    public class AddressImport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddressImport"/> class.
        /// </summary>
        public AddressImport()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressImport" /> class.
        /// </summary>
        /// <param name="groupLocationTypeValueId">The group location type value identifier. (Home, Work, etc)</param>
        /// <param name="street">The street.</param>
        /// <param name="city">The city.</param>
        /// <param name="state">The state.</param>
        /// <param name="postalCode">The postal code.</param>
        /// <param name="country">The country.</param>
        public AddressImport( int groupLocationTypeValueId, string street, string city, string state, string postalCode, string country = null ) : this()
        {
            this.GroupLocationTypeValueId = groupLocationTypeValueId;
            this.Street1 = street;
            this.City = city;
            this.State = state;
            this.PostalCode = postalCode;
            this.Country = country;
        }

        /// <summary>
        /// Gets or sets the location type value identifier.
        /// </summary>
        /// <value>
        /// The location type value identifier.
        /// </value>
        [DefinedValue( SystemGuid.DefinedType.GROUP_LOCATION_TYPE )]
        public int GroupLocationTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the <see cref="Rock.Model.Location"/> referenced by this GroupLocation is the mailing address/location for the <see cref="Rock.Model.Group"/>.  
        /// This field is only supported in the UI for family groups
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this is the mailing address/location for this <see cref="Rock.Model.Group"/>.
        /// </value>
        public bool IsMailingLocation { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this is the mappable location for this 
        /// NOTE: Rock requires that exactly one of the Addresses of a Family is the mapped location, so BulkUpdate will do as follows
        /// 1) If exactly one of the AddressImport records is IsMappedLocation=true, that address will be stored in Rock as IsMappedLocation=true
        /// 2) If more than one AddressImport records is IsMappedLocation=true, the "first" IsMappedLocation one be stored in Rock as IsMappedLocation=true
        /// 3) If none of AddressImport records is IsMappedLocation=true, the "first" one be stored in Rock as IsMappedLocation=true
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is location; otherwise, <c>false</c>.
        /// </value>
        public bool IsMappedLocation { get; set; }

        /// <summary>
        /// Gets or sets the first line of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the First line of the Location's Street/Mailing Address. If the Location does not have
        /// a Street/Mailing address, this value is null.
        /// </value>
        [MaxLength( 100 )]
        public string Street1 { get; set; }

        /// <summary>
        /// Gets or sets the second line of the Location's Street/Mailing Address. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the second line of the Location's Street/Mailing Address. if this Location does not have 
        /// Street/Mailing Address or if the address does not have a 2nd line, this value is null.
        /// </value>
        [MaxLength( 100 )]
        public string Street2 { get; set; }

        /// <summary>
        /// Gets or sets the city component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the city component of the Location's Street/Mailing Address. If this Location does not have
        /// a Street/Mailing Address this value will be null.
        /// </value>
        [MaxLength( 50 )]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the county.
        /// </summary>
        /// <value>
        /// The county.
        /// </value>
        [MaxLength( 50 )]
        public string County { get; set; }

        /// <summary>
        /// Gets or sets the State component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the state component of the Location's Street/Mailing Address. If this Location does not have 
        /// a Street/Mailing Address, this value will be null.
        /// </value>
        [MaxLength( 50 )]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the country component of the Location's Street/Mailing Address. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the country component of the Location's Street/Mailing Address. If this Location does not have a 
        /// Street/Mailing Address, this value will be null.
        /// </value>
        [MaxLength( 50 )]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the Zip/Postal Code component of the Location's Street/Mailing Address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Zip/Postal Code component of the Location's Street/Mailing Address. If this Location does not have 
        /// Street/Mailing Address, this value will be null.
        /// </value>
        [MaxLength( 50 )]
        public string PostalCode { get; set; }
    }
}
