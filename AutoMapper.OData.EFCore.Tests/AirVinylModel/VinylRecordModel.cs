using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoMapper.OData.EFCore.Tests.AirVinylModel
{
    public class VinylRecordModel
    {
        [Key]
        public int VinylRecordId { get; set; }

        [StringLength(150)]
        [Required]
        public string Title { get; set; }

        [StringLength(150)]
        [Required]
        public string Artist { get; set; }

        [StringLength(50)]
        public string CatalogNumber { get; set; }

        public int? Year { get; set; }

        public PressingDetailModel PressingDetail { get; set; }

        public int PressingDetailId { get; set; }

        public virtual PersonModel Person { get; set; }

        public int PersonId { get; set; }

        public ICollection<DynamicPropertyModel> DynamicVinylRecordProperties { get; set; } 
            = new List<DynamicPropertyModel>();

        public IDictionary<string, object> Properties { get; set; }

        private Dictionary<string, VinylLinkModel> _links;
        public IDictionary<string, VinylLinkModel> Links { 
            get 
            {
                if (_links is null)
                {
                    _links = new Dictionary<string, VinylLinkModel>()
                    {
                        { "buyingLink", new VinylLinkModel { Href = $"http://test/buy/{VinylRecordId}" } },
                        { "reviewLink", new VinylLinkModel { Href = $"http://test/review/{VinylRecordId}" } }
                    };
                }

                return _links;
            } 
        }

        private Dictionary<string, VinylLinkModel> _moreLinks;
        public Dictionary<string, VinylLinkModel> MoreLinks
        {
            get
            {
                if (_moreLinks is null)
                {
                    _moreLinks = new Dictionary<string, VinylLinkModel>()
                    {
                        { "buyingLink", new VinylLinkModel { Href = $"http://test/buy/{VinylRecordId}" } },
                        { "reviewLink", new VinylLinkModel { Href = $"http://test/review/{VinylRecordId}" } }
                    };
                }

                return _moreLinks;
            }
        }

        private SortedDictionary<string, VinylLinkModel> _extraLinks;
        public SortedDictionary<string, VinylLinkModel> ExtraLinks
        {
            get
            {
                if (_extraLinks is null)
                {
                    _extraLinks = new SortedDictionary<string, VinylLinkModel>()
                    {
                        { "buyingLink", new VinylLinkModel { Href = $"http://test/buy/{VinylRecordId}" } },
                        { "reviewLink", new VinylLinkModel { Href = $"http://test/review/{VinylRecordId}" } }
                    };
                }

                return _extraLinks;
            }
        }

        private System.Collections.Concurrent.ConcurrentDictionary<string, VinylLinkModel> _additionalLinks;
        public System.Collections.Concurrent.ConcurrentDictionary<string, VinylLinkModel> AdditionalLinks
        {
            get
            {
                if (_additionalLinks is null)
                {
                    _additionalLinks = new System.Collections.Concurrent.ConcurrentDictionary<string, VinylLinkModel>();
                    _additionalLinks.TryAdd("buyingLink", new VinylLinkModel { Href = $"http://test/buy/{VinylRecordId}" });
                    _additionalLinks.TryAdd("reviewLink", new VinylLinkModel { Href = $"http://test/review/{VinylRecordId}" });
                }

                return _additionalLinks;
            }
        }
    }
}
