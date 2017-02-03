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
            var fileName = Server.MapPath( "~/App_Data/ImportUpdate.csv" );
            var fileStream = File.OpenText( fileName );
            CsvReader csv = new CsvReader( fileStream );
            csv.Configuration.HasHeaderRecord = false;

            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var groupService = new GroupService( rockContext );
            int familyGroupTypeId = GroupTypeCache.GetFamilyGroupType().Id;
            int personRecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;

            // dictionary of Families. KEY is FamilyForeignId
            Dictionary<int, Group> familiesLookup = groupService.Queryable().AsNoTracking().Where( a => a.GroupTypeId == familyGroupTypeId && a.ForeignId.HasValue )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v );

            Dictionary<int, Person> personLookup = personService.Queryable().AsNoTracking().Where( a => a.ForeignId.HasValue )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v );

            while ( csv.Read() )
            {
                var record = csv.GetRecord<ImportUpdateRow>();
                Group family = null;

                if ( familiesLookup.ContainsKey( record.FamilyForeignId ) )
                {
                    family = familiesLookup[record.FamilyForeignId];
                }

                if ( family == null )
                {
                    family = new Group();
                    family.GroupTypeId = familyGroupTypeId;
                    family.Name = record.FamilyName;
                    family.ForeignId = record.FamilyForeignId;
                    familiesLookup.Add( record.FamilyForeignId, family );
                }

                Person person = null;
                if ( personLookup.ContainsKey( record.PersonForeignId ) )
                {
                    person = personLookup[record.PersonForeignId];
                }

                if ( person == null )
                {
                    person = new Person();
                    //person.RecordTypeValueId = TODO...
                    person.FirstName = record.FirstName;
                    person.NickName = string.IsNullOrEmpty( record.NickName ) ? record.FirstName : record.NickName;
                    person.LastName = record.LastName;
                    person.ConnectionStatusValueId = record.ConnectionStatusValueId;
                    person.RecordTypeValueId = personRecordTypeValueId;

                    person.ForeignId = record.PersonForeignId;
                    personLookup.Add( record.PersonForeignId, person );
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
            public int FamilyForeignId { get; set; }
            public string FamilyName { get; set; }

            public int PersonForeignId { get; set; }

            public string FirstName { get; set; }
            public string NickName { get; set; }
            public string LastName { get; set; }
            public string MiddleName { get; set; }

            public int? ConnectionStatusValueId { get; set; }
        }

        // Fluent Mapping of CSV file to ImportUpdateRow class
        public sealed class ImportUpdateRowMap : CsvClassMap<ImportUpdateRow>
        {
            public ImportUpdateRowMap()
            {
                Map( m => m.FamilyForeignId );
                Map( m => m.FamilyName );
                Map( m => m.PersonForeignId );
                Map( m => m.FirstName );
                Map( m => m.NickName );
                Map( m => m.LastName );
                Map( m => m.MiddleName );
                Map( m => m.ConnectionStatusValueId );
            }
        }
    }
}