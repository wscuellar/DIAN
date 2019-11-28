using Microsoft.Azure.Documents;
using System.Collections.Generic;

namespace Gosocket.Dian.Application.Cosmos
{
    public class DocumentCollectionSpec
    {
        /// <summary>
        /// Gets or sets the IndexingPolicy to use.
        /// </summary>
        public IndexingPolicy IndexingPolicy { get; set; }

        /// <summary>
        /// Gets or sets the OfferType to use, e.g., S1, S2, S3.
        /// </summary>
        public string OfferType { get; set; }

        /// <summary>
        /// Gets or sets the stored procedures to register.
        /// </summary>
        public IList<StoredProcedure> StoredProcedures { get; set; }

        /// <summary>
        /// Gets or sets the triggers to register.
        /// </summary>
        public IList<Trigger> Triggers { get; set; }

        /// <summary>
        /// Gets or sets the UDFs to register.
        /// </summary>
        public IList<UserDefinedFunction> UserDefinedFunctions { get; set; }
    }
}
