using AutoMapper.AspNet.OData;
using AutoMapper.OData.EFCore.Tests.AirVinylData;
using AutoMapper.OData.EFCore.Tests.AirVinylModel;
using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AutoMapper.OData.EFCore.Tests
{
    public class ExpansionTests : IClassFixture<ExpansionTestsFixture>
    {
        private readonly ExpansionTestsFixture _fixture;

        public ExpansionTests(ExpansionTestsFixture fixture)
        {
            _fixture = fixture;
            serviceProvider = _fixture.ServiceProvider;
        }

        #region Fields
        private readonly IServiceProvider serviceProvider;
        #endregion Fields

        [Fact]
        public async Task GetPeopleExpandsComplexTypesByDefault()
        {
            string query = "/personmodel";
            Test(await GetAsync<PersonModel, Person>(query));

            void Test(ICollection<PersonModel> collection)
            {
                Assert.True(collection.Count > 0);
                Assert.True(collection.All(r => r.VinylRecords.Count == 0));
                Assert.NotEmpty(collection.First().Cars);
            }
        }

        [Fact]
        public async Task GetVinylRecordsExpandsComplexTypesByDefault()
        {
            string query = "/vinylrecordmodel";
            Test(await GetAsync<VinylRecordModel, VinylRecord>(query));

            void Test(ICollection<VinylRecordModel> collection)
            {
                Assert.True(collection.Count > 0);

                //Navigation properties
                Assert.True(collection.All(vinyl => vinyl.Person is null));
                Assert.True(collection.All(vinyl => vinyl.PressingDetail is null));
                
                //Complex types
                Assert.Contains(collection, vinyl => vinyl.Properties.Count != 0);
                Assert.Contains(collection, vinyl => vinyl.Properties.Any(p => !p.Value.GetType().IsLiteralType()));
                Assert.Contains(collection, vinyl => vinyl.DynamicVinylRecordProperties.Count != 0);
                Assert.Contains(collection, vinyl => vinyl.Links.Count != 0);
            }
        }

        [Fact]
        public async Task ExpandingComplexTypesSupportsAllGenericDictionaries()
        {
            string query = "/vinylrecordmodel";
            Test(await GetAsync<VinylRecordModel, VinylRecord>(query));

            void Test(ICollection<VinylRecordModel> collection)
            {
                Assert.True(collection.Count > 0);
                Assert.Contains(collection, vinyl => vinyl.Links.Count != 0);
                Assert.Contains(collection, vinyl => vinyl.MoreLinks.Count != 0);
                Assert.Contains(collection, vinyl => vinyl.ExtraLinks.Count != 0);
                Assert.Contains(collection, vinyl => vinyl.AdditionalLinks.Count != 0);
            }
        }

        [Fact]
        public async Task GetRecordStoresExpandsComplexTypesByDefault()
        {
            string query = "/recordstoremodel";
            Test(await GetAsync<RecordStoreModel, RecordStore>(query));

            static void Test(ICollection<RecordStoreModel> collection)
            {
                Assert.True(collection.Count > 0);
                Assert.True(collection.All(r => r.Ratings.Count == 0));
                Assert.False(string.IsNullOrEmpty(collection.First().StoreAddress.Country));
                Assert.Empty(collection.First().StoreAddress.Doors);
            }
        }

        [Fact]
        public async Task GetRecordStoresCanExpandNavigationPropertyUnderComplexType()
        {
            string query = "/recordstoremodel?$expand=StoreAddress/Doors";
            Test(await GetAsync<RecordStoreModel, RecordStore>(query));

            static void Test(ICollection<RecordStoreModel> collection)
            {
                Assert.True(collection.Count > 0);
                Assert.True(collection.All(r => r.Ratings.Count == 0));
                Assert.False(string.IsNullOrEmpty(collection.First().StoreAddress.Country));
                Assert.NotEmpty(collection.First().StoreAddress.Doors);
                Assert.False(string.IsNullOrEmpty(collection.First().StoreAddress.Doors.First().Name));
                Assert.Null(collection.First().StoreAddress.Doors.First().DoorManufacturer);
            }
        }

        [Fact]
        public async Task GetRecordStoresCanExpandNavigationPropertyOfNavigationPropertyUnderComplexType()
        {
            string query = "/recordstoremodel?$expand=StoreAddress/Doors($expand=DoorManufacturer)";
            Test(await GetAsync<RecordStoreModel, RecordStore>(query));

            static void Test(ICollection<RecordStoreModel> collection)
            {
                Assert.True(collection.Count > 0);
                Assert.True(collection.All(r => r.Ratings.Count == 0));
                Assert.False(string.IsNullOrEmpty(collection.First().StoreAddress.Country));
                Assert.NotEmpty(collection.First().StoreAddress.Doors);
                Assert.False(string.IsNullOrEmpty(collection.First().StoreAddress.Doors.First().Name));
                Assert.NotNull(collection.First().StoreAddress.Doors.First().DoorManufacturer);
                Assert.False(string.IsNullOrEmpty(collection.First().StoreAddress.Doors.First().DoorManufacturer.Name));
                Assert.Null(collection.First().StoreAddress.Doors.First().DoorKnob);
            }
        }

        [Fact]
        public async Task GetRecordStoresCanExpandMultipleNavigationPropertiesOfNavigationPropertyUnderComplexType()
        {
            string query = "/recordstoremodel?$expand=StoreAddress/Doors($expand=DoorManufacturer,DoorKnob)";
            Test(await GetAsync<RecordStoreModel, RecordStore>(query));

            static void Test(ICollection<RecordStoreModel> collection)
            {
                Assert.True(collection.Count > 0);
                Assert.True(collection.All(r => r.Ratings.Count == 0));
                Assert.False(string.IsNullOrEmpty(collection.First().StoreAddress.Country));
                Assert.NotEmpty(collection.First().StoreAddress.Doors);
                Assert.False(string.IsNullOrEmpty(collection.First().StoreAddress.Doors.First().Name));
                Assert.NotNull(collection.First().StoreAddress.Doors.First().DoorManufacturer);
                Assert.False(string.IsNullOrEmpty(collection.First().StoreAddress.Doors.First().DoorManufacturer.Name));
                Assert.NotNull(collection.First().StoreAddress.Doors.First().DoorKnob);
                Assert.False(string.IsNullOrEmpty(collection.First().StoreAddress.Doors.First().DoorKnob.Style));
            }
        }

        private async Task<ICollection<TModel>> GetAsync<TModel, TData>(string query, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null, string customNamespace = null) where TModel : class where TData : class
        {
            return await GetAsync
            (
                query,
                serviceProvider.GetRequiredService<AirVinylDbContext>().Set<TData>(),
                options,
                querySettings,
                customNamespace
            );
        }

        private async Task<ICollection<TModel>> GetAsync<TModel, TData>(string query,
            IQueryable<TData> dataQueryable, ODataQueryOptions<TModel> options = null, QuerySettings querySettings = null, string customNamespace = null) where TModel : class where TData : class
        {
            return
            (
                await DoGet
                (
                    serviceProvider.GetRequiredService<IMapper>()
                )
            ).ToList();

            async Task<IQueryable<TModel>> DoGet(IMapper mapper)
            {
                return await dataQueryable.GetQueryAsync
                (
                    mapper,
                    options ?? GetODataQueryOptions<TModel>(query, customNamespace),
                    querySettings
                );
            }
        }

        private ODataQueryOptions<TModel> GetODataQueryOptions<TModel>(string query, string customNamespace = null) where TModel : class
        {
            return ODataHelpers.GetODataQueryOptions<TModel>
            (
                query,
                serviceProvider,
                customNamespace
            );
        }
    }

    public class ExpansionTestsFixture
    {
        public ExpansionTestsFixture()
        {
            IServiceCollection services = new ServiceCollection();
            IMvcCoreBuilder builder = new TestMvcCoreBuilder
            {
                Services = services
            };

            builder.AddOData();
            services.AddDbContext<AirVinylDbContext>
                (
                    options => options.UseSqlServer
                    (
                        @"Server=(localdb)\mssqllocaldb;Database=ExpansionTestsDatabase;ConnectRetryCount=0",
                        options => options.EnableRetryOnFailure()
                    ),
                    ServiceLifetime.Transient
                )
                .AddSingleton<IConfigurationProvider>(new MapperConfiguration(cfg => cfg.AddMaps(typeof(GetTests).Assembly)))
                .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<IConfigurationProvider>(), sp.GetService))
                .AddTransient<IApplicationBuilder>(sp => new ApplicationBuilder(sp))
                .AddRouting()
                .AddLogging();

            ServiceProvider = services.BuildServiceProvider();

            AirVinylDbContext context = ServiceProvider.GetRequiredService<AirVinylDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            AirVinylDatabaseInitializer.SeedDatabase(context);
        }

        internal IServiceProvider ServiceProvider;
    }
}
