using System;

namespace Rock.BulkUpdate
{
    /// <summary>
    /// 
    /// </summary>
    public class AttributeValueImport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueImport"/> class.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="value">The value.</param>
        public AttributeValueImport( int attributeId, string value )
        {
            this.AttributeId = attributeId;
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueImport"/> class.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="value">The value.</param>
        public AttributeValueImport( int attributeId, DateTime? value )
            : this( attributeId, value != null ? value.Value.ToString( "o" ) : null )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueImport"/> class.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public AttributeValueImport( int attributeId, bool? value )
            : this( attributeId, value != null ? value.Value.ToTrueFalse() : null )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueImport"/> class.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="value">The value.</param>
        public AttributeValueImport( int attributeId, int? value )
            : this( attributeId, value.ToString() )
        {
        }

        /// <summary>
        /// Gets or sets the attribute identifier.
        /// </summary>
        /// <value>
        /// The attribute identifier.
        /// </value>
        public int AttributeId { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }
    }
}
