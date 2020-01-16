using System.Collections.Generic;

namespace Dapper
{
    /// <summary>
    /// An enumerable collection of T with paging information
    /// </summary>
    /// <typeparam name="T">The type of the row objects</typeparam>
    public class PagedList<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class.
        /// <typeparamref name="T">The type of the list objects.</typeparamref>
        /// </summary>
        public PagedList()
        {
            this.HasNext = false;
            this.HasPrevious = false;
            this.Rows = new T[0];
            this.TotalRows = 0;
            this.TotalPages = 0;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has a next page.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has a previous page; otherwise, <c>false</c>.
        /// </value>
        public bool HasNext { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has a previous page.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has a previous page; otherwise, <c>false</c>.
        /// </value>
        public bool HasPrevious { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        /// <value>
        /// The total number of pages.
        /// </value>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets the total number of rows.
        /// </summary>
        /// <value>
        /// The total number of rows.
        /// </value>
        public int TotalRows { get; set; }

        /// <summary>
        /// Gets or sets the rows for this page.
        /// </summary>
        /// <value>
        /// The rows for this page.
        /// </value>
        public IEnumerable<T> Rows { get; set; }
    }
}
