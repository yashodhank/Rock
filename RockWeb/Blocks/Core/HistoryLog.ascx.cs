﻿// <copyright>
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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Block for displaying the history of changes to a particular entity.
    /// </summary>
    [DisplayName( "History Log" )]
    [Category( "Core" )]
    [Description( "Block for displaying the history of changes to a particular entity." )]

    [ContextAware]
    public partial class HistoryLog : RockBlock
    {

        #region Fields

        private IEntity _entity = null;

        #endregion

        #region Base Control Methods


        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;
            gfSettings.DisplayFilterValue += gfSettings_DisplayFilterValue;

            gHistory.DataKeyNames = new string[] { "Id" };
            gHistory.GridRebind += gHistory_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            _entity = this.ContextEntity();
            if ( _entity != null )
            {
                lHeading.Text = string.Format( "{0} (ID:{1})", _entity.ToString(), _entity.Id );

                IModel model = _entity as IModel;
                if ( !Page.IsPostBack )
                {
                    BindFilter();
                    BindGrid();

                    if ( model != null && model.CreatedDateTime.HasValue )
                    {
                        hlDateAdded.Text = String.Format( "Date Created: {0}", model.CreatedDateTime.Value.ToShortDateString() );
                    }
                    else
                    {
                        hlDateAdded.Visible = false;
                    }
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            int? categoryId = cpCategory.SelectedValueAsInt();
            gfSettings.SaveUserPreference( "Category", categoryId.HasValue ? categoryId.Value.ToString() : "" );
            
            gfSettings.SaveUserPreference( "Summary Contains", tbSummary.Text );

            int? personId = ppWhoFilter.PersonId;
            gfSettings.SaveUserPreference( "Who", personId.HasValue ? personId.ToString() : string.Empty );
            
            gfSettings.SaveUserPreference( "Date Range", drpDates.DelimitedValues );

            BindGrid();
        }

        /// <summary>
        /// Handles the DisplayFilterValue event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void gfSettings_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Category":
                    {
                        int? categoryId = e.Value.AsIntegerOrNull();
                        if ( categoryId.HasValue )
                        {
                            var category = Rock.Web.Cache.CategoryCache.Read( categoryId.Value );
                            if ( category != null )
                            {
                                e.Value = category.Name;
                            }
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }
                case "Summary Contains":
                    {
                        break;
                    }
                case "Who":
                    {
                        int personId = int.MinValue;
                        if ( int.TryParse( e.Value, out personId ) )
                        {
                            var person = new PersonService( new RockContext() ).Get( personId );
                            if ( person != null )
                            {
                                e.Value = person.FullName;
                            }
                        }
                        break;
                    }
                case "Date Range":
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }
                default:
                    {
                        e.Value = string.Empty;
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gHistory_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            int? categoryId = gfSettings.GetUserPreference( "Category" ).AsIntegerOrNull();
            cpCategory.SetValue( categoryId );

            tbSummary.Text = gfSettings.GetUserPreference( "Summary Contains" );
            int personId = int.MinValue;
            if ( int.TryParse( gfSettings.GetUserPreference( "Who" ), out personId ) )
            {
                var person = new PersonService( new RockContext() ).Get( personId );
                if ( person != null )
                {
                    ppWhoFilter.SetValue( person );
                }
                else
                {
                    gfSettings.SaveUserPreference( "Who", string.Empty );
                }
            }
            drpDates.DelimitedValues = gfSettings.GetUserPreference( "Date Range" );

        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            if ( _entity != null )
            {
                var entityTypeCache = EntityTypeCache.Read( _entity.GetType(), false );
                if ( entityTypeCache != null )
                {
                    var qry = new HistoryService( new RockContext() ).Queryable( "CreatedByPersonAlias.Person" )
                        .Where( h =>
                            ( h.EntityTypeId == entityTypeCache.Id && h.EntityId == _entity.Id ) );

                    int? categoryId = gfSettings.GetUserPreference( "Category" ).AsIntegerOrNull();
                    if ( categoryId.HasValue )
                    {
                        qry = qry.Where( a => a.CategoryId == categoryId.Value );
                    }

                    string summary = gfSettings.GetUserPreference( "Summary Contains" );
                    if ( !string.IsNullOrWhiteSpace( summary ) )
                    {
                        qry = qry.Where( h => h.Summary.Contains( summary ) );
                    }

                    int personId = int.MinValue;
                    if ( int.TryParse( gfSettings.GetUserPreference( "Who" ), out personId ) )
                    {
                        qry = qry.Where( h => h.CreatedByPersonAlias.PersonId == personId );
                    }

                    var drp = new DateRangePicker();
                    drp.DelimitedValues = gfSettings.GetUserPreference( "Date Range" );
                    if ( drp.LowerValue.HasValue )
                    {
                        qry = qry.Where( h => h.CreatedDateTime >= drp.LowerValue.Value );
                    }
                    if ( drp.UpperValue.HasValue )
                    {
                        DateTime upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                        qry = qry.Where( h => h.CreatedDateTime < upperDate );
                    }

                    SortProperty sortProperty = gHistory.SortProperty;
                    if ( sortProperty != null )
                    {
                        qry = qry.Sort( sortProperty );
                    }
                    else
                    {
                        qry = qry.OrderByDescending( t => t.CreatedDateTime );
                    }

                    // Combine history records that were saved at the same time
                    var histories = new List<History>();
                    foreach ( var history in qry )
                    {
                        var existingHistory = histories
                            .Where( h =>
                                h.CreatedByPersonAliasId == history.CreatedByPersonAliasId &&
                                h.CreatedDateTime == history.CreatedDateTime &&
                                h.EntityTypeId == history.EntityTypeId &&
                                h.EntityId == history.EntityId &&
                                h.CategoryId == history.CategoryId &&
                                h.RelatedEntityTypeId == history.RelatedEntityTypeId &&
                                h.RelatedEntityId == history.RelatedEntityId ).FirstOrDefault();
                        if ( existingHistory != null )
                        {
                            existingHistory.Summary += "<br/>" + history.Summary;
                        }
                        else
                        {
                            histories.Add( history );
                        }
                    }

                    gHistory.DataSource = histories.Select( h => new
                    {
                        Id = h.Id,
                        CategoryId = h.CategoryId,
                        Category = h.Category != null ? h.Category.Name : "",
                        EntityTypeId = h.EntityTypeId,
                        EntityId = h.EntityId,
                        Caption = h.Caption ?? string.Empty,
                        Summary = h.Summary,
                        RelatedEntityTypeId = h.RelatedEntityTypeId ?? 0,
                        RelatedEntityId = h.RelatedEntityId ?? 0,
                        CreatedByPersonId = h.CreatedByPersonAlias != null ? h.CreatedByPersonAlias.PersonId : 0,
                        PersonName = h.CreatedByPersonAlias != null && h.CreatedByPersonAlias.Person != null ? h.CreatedByPersonAlias.Person.NickName + " " + h.CreatedByPersonAlias.Person.LastName : "",
                        CreatedDateTime = h.CreatedDateTime
                    } ).ToList();

                    gHistory.EntityTypeId = EntityTypeCache.Read<History>().Id;
                    gHistory.DataBind();
                }
            }

        }

        /// <summary>
        /// Formats the caption.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="relatedEntityId">The related entity identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns></returns>
        protected string FormatCaption( int categoryId, string caption, int? relatedEntityId, int? entityId = null )
        {
            var category = CategoryCache.Read( categoryId );
            if (category != null)
            {
                string urlMask = category.GetAttributeValue( "UrlMask" );
                if (!string.IsNullOrWhiteSpace(urlMask))
                {
                    if (urlMask.Contains("{0}"))
                    {
                        string p1 = relatedEntityId.HasValue ? relatedEntityId.Value.ToString() : "";
                        string p2 = entityId.HasValue ? entityId.Value.ToString() : "";
                        urlMask = string.Format( urlMask, p1, p2 );
                    }
                    return string.Format( "<a href='{0}'>{1}</a>", ResolveRockUrl( urlMask ), caption );
                }
            }

            return caption;
        }

        #endregion
    }
}