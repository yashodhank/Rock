// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Web.OData;

namespace Rock.Rest
{
    /// <summary>
    /// This class defines an attribute that can be applied to an action to enable
    /// querying using the OData query syntax using the precompiled Rock EdmModel
    /// </summary>
    /// <seealso cref="System.Web.OData.EnableQueryAttribute" />
    public class RockEnableQueryAttribute : EnableQueryAttribute
    {
        /// <summary>
        /// Gets the EDM model for the given type and request. Override this method to customize the EDM model used for querying.
        /// </summary>
        /// <param name="elementClrType">The CLR type to retrieve a model for.</param>
        /// <param name="request">The request message to retrieve a model for.</param>
        /// <param name="actionDescriptor">The action descriptor for the action being queried on.</param>
        /// <returns>
        /// The EDM model for the given type and request.
        /// </returns>
        public override Microsoft.OData.Edm.IEdmModel GetModel( Type elementClrType, System.Net.Http.HttpRequestMessage request, System.Web.Http.Controllers.HttpActionDescriptor actionDescriptor )
        {
            // use the EdmModel that we already created in WebApiConfig (so that we don't have problems with OData4 Open Types)
            return WebApiConfig.EdmModel;
        }
    }
}
