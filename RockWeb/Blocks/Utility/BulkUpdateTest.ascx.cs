// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using CsvHelper;
using CsvHelper.Configuration;
using Rock.BulkUpdate;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "BulkUpdateTest" )]
    [Category( "Utility" )]
    [Description( "" )]
    public partial class BulkUpdateTest : Rock.Web.UI.RockBlock
    {
        #region

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                // added for your convenience

                // to show the created/modified by date time details in the PanelDrawer do something like this:
                // pdAuditDetails.SetEntity( <YOUROBJECT>, ResolveRockUrl( "~" ) );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            //
        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)

        #endregion

        protected void btnGo_Click( object sender, EventArgs e )
        {


            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var groupService = new GroupService( rockContext );
            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            int familyGroupTypeId = familyGroupType.Id;
            int adultRoleId = familyGroupType.Roles.Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).First().Id;

            int personRecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;

            int recordStatusValueActiveId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
            int recordStatusValueInActiveId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;

            int connectionStatusValueAttendeeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_ATTENDEE.AsGuid() ).Id;

            int maritalStatusMarriedId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() ).Id;
            int maritalStatusSingleId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() ).Id;

            int phoneNumberTypeHomeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() ).Id;
            int phoneNumberTypeMobileId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Id;
            int phoneNumberTypeWorkId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() ).Id;

            int groupLocationTypeHomeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() ).Id;
            int groupLocationTypeWorkId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid() ).Id;


            var attributeService = new AttributeService( rockContext );
            var attributeAllergy = attributeService.GetByEntityTypeId( EntityTypeCache.Read<Person>().Id ).FirstOrDefault( a => a.Key == "Allergy" );
            var attributeBaptismDate = attributeService.GetByEntityTypeId( EntityTypeCache.Read<Person>().Id ).FirstOrDefault( a => a.Key == "BaptismDate" );

            var gradeOffsetLookup = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() ).DefinedValues.ToDictionary( k => k.Description.ToLower(), v => v.Value.AsInteger() );

            // dictionary of Families. KEY is FamilyForeignId
            Dictionary<int, Group> familiesLookup = groupService.Queryable().AsNoTracking().Where( a => a.GroupTypeId == familyGroupTypeId && a.ForeignId.HasValue )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v );

            Dictionary<int, Person> personLookup = personService.Queryable().AsNoTracking().Where( a => a.ForeignId.HasValue )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v );

            var personImports = new List<PersonImport>();
            var campusLookup = CampusCache.All().ToDictionary( k => k.Name, v => v.Id );

            var fileName = Server.MapPath( "~/App_Data/export_individuals.csv" );
            using ( var fileStream = File.OpenText( fileName ) )
            {
                CsvReader csv = new CsvReader( fileStream );
                csv.Configuration.HasHeaderRecord = true;
                csv.Configuration.IgnoreHeaderWhiteSpace = true;
                csv.Configuration.IsHeaderCaseSensitive = false;
                csv.Configuration.RegisterClassMap<ImportUpdateRowMap>();
                csv.Configuration.BadDataCallback = ( s ) =>
                {
                    System.Diagnostics.Debug.WriteLine( s );
                };

                foreach ( var flatRecord in csv.GetRecords<ImportUpdateRow>() )
                {
                    var personImport = new PersonImport
                    {
                        PersonForeignId = flatRecord.IndividualID.AsInteger(),
                        FamilyForeignID = flatRecord.FamilyID.AsInteger(),
                        GroupRoleId = adultRoleId,
                        GivingIndividually = false,
                        RecordTypeValueId = personRecordTypeValueId,

                    };

                    if (personImport.PersonForeignId == 0 || personImport.FamilyForeignID == 0)
                    {
                        continue;
                    }

                    if ( !string.IsNullOrEmpty( flatRecord.Campus ) && campusLookup.ContainsKey( flatRecord.Campus ) )
                    {
                        personImport.CampusId = campusLookup[flatRecord.Campus];
                    }

                    if ( flatRecord.Inactive.Equals( "Inactive", StringComparison.OrdinalIgnoreCase ) )
                    {
                        personImport.RecordStatusReasonValueId = recordStatusValueInActiveId;
                    }
                    else
                    {
                        personImport.RecordStatusReasonValueId = recordStatusValueActiveId;
                    }

                    personImport.ConnectionStatusValueId = connectionStatusValueAttendeeId;

                    personImport.IsDeceased = flatRecord.Deceased_Date.AsDateTime().HasValue;

                    personImport.FirstName = flatRecord.LegalFirst;
                    personImport.NickName = flatRecord.First;
                    personImport.MiddleName = flatRecord.Middle;
                    personImport.LastName = flatRecord.Last;

                    var birthDate = flatRecord.Birthday.AsDateTime();
                    if ( birthDate != null )
                    {
                        personImport.BirthDay = birthDate.Value.Day;
                        personImport.BirthMonth = birthDate.Value.Month;
                        personImport.BirthYear = birthDate.Value.Year;
                    }

                    personImport.Gender = flatRecord.Gender.ConvertToEnumOrNull<Gender>() ?? Gender.Unknown;


                    if ( flatRecord.MaritalStatus.Equals( "Married", StringComparison.OrdinalIgnoreCase ) )
                    {
                        personImport.MaritalStatusValueId = maritalStatusMarriedId;
                    }
                    else if ( flatRecord.MaritalStatus.Equals( "Single", StringComparison.OrdinalIgnoreCase ) )
                    {
                        personImport.MaritalStatusValueId = maritalStatusSingleId;
                    }

                    personImport.AnniversaryDate = flatRecord.Anniversary.AsDateTime();

                    // determine GraduationYear from "School Grade"
                    if ( !string.IsNullOrEmpty( flatRecord.SchoolGrade ) )
                    {
                        var schoolGradeKey = flatRecord.SchoolGrade.ToLower();
                        if ( gradeOffsetLookup.ContainsKey( schoolGradeKey ) )
                        {
                            personImport.GraduationYear = Person.GraduationYearFromGradeOffset( gradeOffsetLookup[schoolGradeKey] );
                        }
                    }

                    personImport.Email = flatRecord.Email;
                    if ( flatRecord.EmailPrivacyLevel == "1" )
                    {
                        personImport.EmailPreference = EmailPreference.EmailAllowed;
                    }
                    else
                    {
                        personImport.EmailPreference = EmailPreference.DoNotEmail;
                    }

                    personImport.CreatedDateTime = flatRecord.Date_Created.AsDateTime();
                    personImport.ModifiedDateTime = flatRecord.Date_Modified.AsDateTime();

                    // Phone Numbers
                    personImport.PhoneNumbers = new List<PhoneNumberImport>();

                    if ( !string.IsNullOrEmpty( flatRecord.HomePhone.AsNumeric() ) )
                    {
                        personImport.PhoneNumbers.Add( new PhoneNumberImport( flatRecord.HomePhone.AsNumeric(), phoneNumberTypeHomeId ) );
                    }

                    if ( !string.IsNullOrEmpty( flatRecord.MobilePhone.AsNumeric() ) )
                    {
                        personImport.PhoneNumbers.Add( new PhoneNumberImport( flatRecord.MobilePhone.AsNumeric(), phoneNumberTypeMobileId ) );
                    }

                    if ( !string.IsNullOrEmpty( flatRecord.WorkPhone.AsNumeric() ) )
                    {
                        personImport.PhoneNumbers.Add( new PhoneNumberImport( flatRecord.WorkPhone.AsNumeric(), phoneNumberTypeWorkId ) );
                    }

                    // Addresses
                    personImport.Addresses = new List<AddressImport>();
                    if ( !string.IsNullOrEmpty( flatRecord.HomeStreet ) )
                    {
                        var homeAddress = new AddressImport( groupLocationTypeHomeId, flatRecord.HomeStreet, flatRecord.HomeCity, flatRecord.HomeState, flatRecord.HomeZip )
                        {
                            IsMailingLocation = false,
                            IsMappedLocation = true
                        };

                        personImport.Addresses.Add( homeAddress );
                    }

                    if ( !string.IsNullOrEmpty( flatRecord.MailingStreet ) )
                    {
                        var mailingAddress = new AddressImport( groupLocationTypeHomeId, flatRecord.MailingStreet, flatRecord.MailingCity, flatRecord.MailingState, flatRecord.MailingZip )
                        {
                            IsMailingLocation = true,
                            IsMappedLocation = false
                        };

                        personImport.Addresses.Add( mailingAddress );
                    }

                    if ( !string.IsNullOrEmpty( flatRecord.WorkStreet ) )
                    {
                        var workAddress = new AddressImport( groupLocationTypeHomeId, flatRecord.WorkStreet, flatRecord.WorkCity, flatRecord.WorkState, flatRecord.WorkZip )
                        {
                            IsMailingLocation = false,
                            IsMappedLocation = false
                        };

                        personImport.Addresses.Add( workAddress );
                    }

                    // Attributes
                    personImport.AttributeValues = new List<AttributeValueImport>();

                    if ( flatRecord.Allergies.IsNotNullOrWhitespace() && attributeAllergy != null )
                    {
                        personImport.AttributeValues.Add( new AttributeValueImport( attributeAllergy.Id, flatRecord.Allergies ) );
                    }

                    if ( flatRecord.Baptized.AsDateTime() != null && attributeBaptismDate != null )
                    {
                        personImport.AttributeValues.Add( new AttributeValueImport( attributeBaptismDate.Id, flatRecord.Baptized.AsDateTime() ) );
                    }

                    personImports.Add( personImport );
                }
            }

            var familiesToInsert = familiesLookup.Where( a => a.Value.Id == 0 ).Select( a => a.Value ).ToList();

            rockContext.BulkInsert( familiesToInsert );
        }

        /// <summary>
        /// 
        /// </summary>
        public class ImportUpdateRow
        {
            #region flat person fields

            public string IndividualID { get; set; }

            public string FamilyID { get; set; }

            public string FamilyPosition { get; set; }

            public string LimitedAccessUser { get; set; }

            public string Prefix { get; set; }

            public string LegalFirst { get; set; }

            public string First { get; set; }

            public string Middle { get; set; }

            public string Last { get; set; }

            public string Suffix { get; set; }

            public string Campus { get; set; }

            public string Email { get; set; }

            public string EmailPrivacyLevel { get; set; }

            public string GeneralCommunication { get; set; }

            #endregion flat person fields


            #region Mailing Address

            public string MailingArea { get; set; }

            public string MailingStreet { get; set; }

            public string MailingCity { get; set; }

            public string MailingState { get; set; }

            public string MailingZip { get; set; }

            public string MailingCarrierRoute { get; set; }

            public string MailingPrivacyLevel { get; set; }

            public string MailingLatitude { get; set; }

            public string MailingLongitude { get; set; }

            #endregion Mailing Address

            #region PhoneNumbers

            public string HomePhone { get; set; }

            public string HomePhonePrivacyLevel { get; set; }

            public string WorkPhone { get; set; }

            public string WorkPhonePrivacyLevel { get; set; }

            public string MobilePhone { get; set; }

            public string MobileCarrier { get; set; }

            public string MobilePhonePrivacyLevel { get; set; }

            public string Fax { get; set; }

            public string FaxPhonePrivacyLevel { get; set; }

            public string PagerPhone { get; set; }

            public string PagerPhonePrivacyLevel { get; set; }

            public string EmergencyPhone { get; set; }

            public string EmergencyPhonePrivacyLevel { get; set; }

            public string EmergencyContactName { get; set; }

            #endregion


            #region flat person fields2

            public string Birthday { get; set; }

            public string BirthdayPrivacyLevel { get; set; }

            public string Anniversary { get; set; }

            public string AnniversaryPrivacyLevel { get; set; }

            public string Gender { get; set; }

            public string GenderPrivacyLevel { get; set; }

            public string GivingNumber { get; set; }

            public string MaritalStatus { get; set; }

            public string MaritalStatusPrivacyLevel { get; set; }

            #endregion flat person fields2

            #region Home Address

            public string HomeArea { get; set; }

            public string HomeStreet { get; set; }

            public string HomeCity { get; set; }

            public string HomeState { get; set; }

            public string HomeZip { get; set; }

            public string HomePrivacyLevel { get; set; }

            #endregion Home Address

            #region Work Address

            public string WorkArea { get; set; }

            public string WorkStreet { get; set; }

            public string WorkCity { get; set; }

            public string WorkState { get; set; }

            public string WorkZip { get; set; }

            public string WorkPrivacyLevel { get; set; }

            #endregion Work Address

            #region Other Address

            public string OtherArea { get; set; }

            public string OtherStreet { get; set; }

            public string OtherCity { get; set; }

            public string OtherState { get; set; }

            public string OtherZip { get; set; }

            public string OtherPrivacyLevel { get; set; }

            #endregion Other Address

            #region Attributes

            public string WorkTitle { get; set; }

            public string SchoolName { get; set; }

            public string SchoolGrade { get; set; }

            public string Allergies { get; set; }

            public string Confirmednoallergies { get; set; }

            public string AllergiesPrivacyLevel { get; set; }

            public string CommitmentDate { get; set; }

            public string CommitmentStory { get; set; }

            public string CurrentStory { get; set; }

            public string MyWebSite { get; set; }

            public string WorkWebSite { get; set; }

            public string Military { get; set; }

            public string Services_usually_attended { get; set; }

            public string Plugged_In_Privacy_Level { get; set; }

            public string Other_Email { get; set; }

            public string Alternate_Phone { get; set; }

            public string Previous_Add { get; set; }

            public string SecureSearch { get; set; }

            public string BGCK_Completed { get; set; }

            public string User_Defined_Text_6 { get; set; }

            public string User_Defined_Text_7 { get; set; }

            public string User_Defined_Text_8 { get; set; }

            public string User_Defined_Text_9 { get; set; }

            public string User_Defined_Text_10 { get; set; }

            public string User_Defined_Text_11 { get; set; }

            public string User_Defined_Text_12 { get; set; }

            public string First_Record { get; set; }

            public string Baby_Dedication { get; set; }

            public string Next { get; set; }

            public string Starting_Point { get; set; }

            public string FPU { get; set; }

            public string GroupLink { get; set; }

            public string untitled1 { get; set; }

            public string untitled2 { get; set; }

            public string untitled3 { get; set; }

            public string untitled4 { get; set; }

            public string untitled5 { get; set; }

            public string untitled6 { get; set; }

            public string Custom_Field_Privacy_Level { get; set; }

            public string Personality_Style { get; set; }

            public string Spiritual_Gifts { get; set; }

            public string Passions { get; set; }

            public string Abilities { get; set; }

            public string My_Fit_Privacy_Level { get; set; }

            #endregion Attributes

            #region UserLogin Meta

            public string Last_logged_in { get; set; }

            #endregion UserLogin Meta

            public string Created_By { get; set; }

            public string Modified_By { get; set; }

            public string Date_Created { get; set; }

            public string Date_Modified { get; set; }

            #region GivingID??

            public string Giving_Number { get; set; }

            #endregion

            #region ??

            public string Listed { get; set; }

            #endregion


            #region ActiveStatus
            public string Inactive { get; set; }
            #endregion


            #region Attributes 2
            public string Membership_Start_Date { get; set; }

            public string Membership_Stop_Date { get; set; }

            public string Membership_Type { get; set; }

            public string Spiritual_Maturity { get; set; }

            public string Baptized { get; set; }

            public string Deceased_Date { get; set; }

            public string How_They_Heard { get; set; }

            public string Reason_Left_Church { get; set; }

            public string Child_Work_Date_Start { get; set; }

            public string Child_Work_Date_Stop { get; set; }

            #endregion Attributes 2

            #region ??

            public string Other_ID { get; set; }

            public string Sync_ID { get; set; }
            #endregion ??

        }

        // Fluent Mapping of CSV file to ImportUpdateRow class
        public sealed class ImportUpdateRowMap : CsvClassMap<ImportUpdateRow>
        {
            public ImportUpdateRowMap()
            {
                Map( m => m.IndividualID );
                Map( m => m.FamilyID );
                Map( m => m.FamilyPosition );
                Map( m => m.LimitedAccessUser );
                Map( m => m.Prefix );
                Map( m => m.LegalFirst ).Name( "Legalfirst" );
                Map( m => m.First );
                Map( m => m.Middle );
                Map( m => m.Last );
                Map( m => m.Suffix );
                Map( m => m.Campus );
                Map( m => m.Email );
                Map( m => m.EmailPrivacyLevel );
                Map( m => m.GeneralCommunication );
                Map( m => m.MailingArea );
                Map( m => m.MailingStreet );
                Map( m => m.MailingCity );
                Map( m => m.MailingState );
                Map( m => m.MailingZip );
                Map( m => m.MailingCarrierRoute );
                Map( m => m.MailingPrivacyLevel );
                Map( m => m.MailingLatitude );
                Map( m => m.MailingLongitude );
                Map( m => m.HomePhone );
                Map( m => m.HomePhonePrivacyLevel );
                Map( m => m.WorkPhone );
                Map( m => m.WorkPhonePrivacyLevel );
                Map( m => m.MobilePhone );
                Map( m => m.MobileCarrier );
                Map( m => m.MobilePhonePrivacyLevel );
                Map( m => m.Fax );
                Map( m => m.FaxPhonePrivacyLevel );
                Map( m => m.PagerPhone );
                Map( m => m.PagerPhonePrivacyLevel );
                Map( m => m.EmergencyPhone );
                Map( m => m.EmergencyPhonePrivacyLevel );
                Map( m => m.EmergencyContactName );
                Map( m => m.Birthday );
                Map( m => m.BirthdayPrivacyLevel );
                Map( m => m.Anniversary );
                Map( m => m.AnniversaryPrivacyLevel );
                Map( m => m.Gender );
                Map( m => m.GenderPrivacyLevel );
                Map( m => m.GivingNumber );
                Map( m => m.MaritalStatus );
                Map( m => m.MaritalStatusPrivacyLevel );
                Map( m => m.HomeArea );
                Map( m => m.HomeStreet );
                Map( m => m.HomeCity );
                Map( m => m.HomeState );
                Map( m => m.HomeZip );
                Map( m => m.HomePrivacyLevel );
                Map( m => m.WorkArea );
                Map( m => m.WorkStreet );
                Map( m => m.WorkCity );
                Map( m => m.WorkState );
                Map( m => m.WorkZip );
                Map( m => m.WorkPrivacyLevel );
                Map( m => m.OtherArea );
                Map( m => m.OtherStreet );
                Map( m => m.OtherCity );
                Map( m => m.OtherState );
                Map( m => m.OtherZip );
                Map( m => m.OtherPrivacyLevel );
                Map( m => m.WorkTitle );
                Map( m => m.SchoolName );
                Map( m => m.SchoolGrade );
                Map( m => m.Allergies );
                Map( m => m.Confirmednoallergies );
                Map( m => m.AllergiesPrivacyLevel );
                Map( m => m.CommitmentDate );
                Map( m => m.CommitmentStory );
                Map( m => m.CurrentStory );
                Map( m => m.MyWebSite );
                Map( m => m.WorkWebSite );
                Map( m => m.Military );
                Map( m => m.Services_usually_attended ).Name( "Service(s)usuallyattended" );
                Map( m => m.Plugged_In_Privacy_Level ).Name( "PluggedInPrivacyLevel" );
                Map( m => m.Other_Email ).Name( "OtherEmail" );
                Map( m => m.Alternate_Phone ).Name( "AlternatePhone" );
                Map( m => m.Previous_Add ).Name( "PreviousAdd" );
                Map( m => m.SecureSearch );
                Map( m => m.BGCK_Completed ).Name( "BGCKCompleted" );
                Map( m => m.User_Defined_Text_6 ).Name( "UserDefined-Text6" );
                Map( m => m.User_Defined_Text_7 ).Name( "UserDefined-Text7" );
                Map( m => m.User_Defined_Text_8 ).Name( "UserDefined-Text8" );
                Map( m => m.User_Defined_Text_9 ).Name( "UserDefined-Text9" );
                Map( m => m.User_Defined_Text_10 ).Name( "UserDefined-Text10" );
                Map( m => m.User_Defined_Text_11 ).Name( "UserDefined-Text11" );
                Map( m => m.User_Defined_Text_12 ).Name( "UserDefined-Text12" );
                Map( m => m.First_Record ).Name( "FirstRecord" );
                Map( m => m.Baby_Dedication ).Name( "BabyDedication" );
                Map( m => m.Next );
                Map( m => m.Starting_Point ).Name( "StartingPoint" );
                Map( m => m.FPU );
                Map( m => m.GroupLink );
                /*Map( m => m.untitled1 );
                Map( m => m.untitled2 );
                Map( m => m.untitled3 );
                Map( m => m.untitled4 );
                Map( m => m.untitled5 );
                Map( m => m.untitled6 );
                */
                Map( m => m.Custom_Field_Privacy_Level ).Name( "CustomFieldPrivacyLevel" );
                Map( m => m.Personality_Style ).Name( "PersonalityStyle" );
                Map( m => m.Spiritual_Gifts ).Name( "SpiritualGifts" );
                Map( m => m.Passions );
                Map( m => m.Abilities );
                Map( m => m.My_Fit_Privacy_Level ).Name( "MyFitPrivacyLevel" );
                Map( m => m.Last_logged_in ).Name( "Lastloggedin" );
                /*Map( m => m.Created_By );
                Map( m => m.Modified_By );
                */
                Map( m => m.Date_Created ).Name( "DateCreated" );
                Map( m => m.Date_Modified ).Name( "DateModified" );
                Map( m => m.Giving_Number ).Name( "GivingNumber" );
                Map( m => m.Listed );
                Map( m => m.Inactive );
                Map( m => m.Membership_Start_Date ).Name( "MembershipStartDate" );
                Map( m => m.Membership_Stop_Date ).Name( "MembershipStopDate" );
                Map( m => m.Membership_Type ).Name( "MembershipType" );
                Map( m => m.Spiritual_Maturity ).Name( "SpiritualMaturity" );
                Map( m => m.Baptized );
                Map( m => m.Deceased_Date ).Name( "DeceasedDate" );
                Map( m => m.How_They_Heard ).Name( "HowTheyHeard" );
                Map( m => m.Reason_Left_Church ).Name( "ReasonLeftChurch" );
                Map( m => m.Child_Work_Date_Start ).Name( "ChildWorkDateStart" );
                Map( m => m.Child_Work_Date_Stop ).Name( "ChildWorkDateStop" );
                Map( m => m.Other_ID ).Name( "OtherID" );
                Map( m => m.Sync_ID ).Name( "SyncID" );
            }
        }
    }
}