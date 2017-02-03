using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;

namespace Rock.BulkUpdate
{
    /// <summary>
    /// 
    /// </summary>
    public class PhoneNumberImport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumberImport"/> class.
        /// </summary>
        public PhoneNumberImport()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumberImport"/> class.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="numberTypeValueId">The number type value identifier.</param>
        public PhoneNumberImport( string number, int numberTypeValueId ) : this()
        {
            this.Number = number;
            this.NumberTypeValueId = numberTypeValueId;
        }

        /// <summary>
        /// Gets or sets the phone number. The number is stored without any string formatting. (i.e. (502) 555-1212 will be stored as 5025551212). This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the phone number without string formatting.
        /// </value>
        [MaxLength( 20 )]
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets the extension (if any) that would need to be dialed to contact the owner. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the extensions that would need to be dialed to contact the owner. If no extension is required, this property will be null. 
        /// </value>
        [MaxLength( 20 )]
        public string Extension { get; set; }

        /// <summary>
        /// Gets the Phone Number's Number Type <see cref="Rock.Model.DefinedValue"/> Id.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Number Type <see cref="Rock.Model.DefinedValue"/> Id. If unknown, this value will be null.
        /// </value>
        [DefinedValue( SystemGuid.DefinedType.PERSON_PHONE_TYPE )]
        public int? NumberTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the number has been opted in for SMS
        /// </summary>
        /// <value>
        ///   A <see cref="System.Boolean"/> value that is <c>true</c> if the phone number has opted in for SMS messaging; otherwise <c>false</c>.
        /// </value>
        [Required]
        public bool IsMessagingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the PhoneNumber is unlisted or not.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the PhoneNumber is unlisted; otherwise <c>false</c>.
        /// </value>
        public bool IsUnlisted { get; set; }
    }
}
