using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using Guru.Collection.Orm;
using Guru.Internal;

namespace Guru.Collection.Episode
{
    [TestFixture]
    public class EpisodeTest
    {
        private EpisodeRepo _episodeRepo;
        private string _insertedId2 = "qiihWlTCtVz72T9znB9";
        private string _insertedId1;
        private DateTimeOffset _record1CreatedAt;

        private DateTimeOffset _dateStarted;

        [OneTimeSetUp, Order(1)]
        public void Start()
        {
            this._episodeRepo = EpisodeRepo.NewEpisodeRepo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Episodes.realm"));
            this._episodeRepo.DeleteAll();

            this._dateStarted = DateTimeOffset.Now;
        }

        [Test, Order(2)]
        public async Task Test_01_Given_CreateRequest_When_Create_Then_InsertOneRecord()
        {
            // test Create 会插入一条记录
            EpisodeCreateReq createReq = new EpisodeCreateReq()
            {
                Name = "测试音频01",
                ContentType = ContentType.SPORTS,
                FilterType = FilterType.MANUAL,
                Count = 1234,
                IsOnline = false,
                Keywords = new[] {"关键词1", "关键词2", "关键词3"},
                Outlines = new Dictionary<string, string> {{"介绍", "介绍部分"}, {"详细内容", "讲解详细内容"}},
                QAs = new List<QuestionAnswer>
                {
                    new() {Question = "问题1", Answer = "问题1的回答内容..."},
                    new() {Question = "问题2", Answer = "问题2的回答内容..."}
                }
            };
            EpisodeCreateRes createRes = await this._episodeRepo.Create(createReq);
            Debug.Assert(createRes != null);
            Assert.IsNotNull(createRes.InsertedId);
            Assert.That(createRes.InsertedCount, Is.EqualTo(1));
            this._insertedId1 = createRes.InsertedId;
            Console.WriteLine("Episode created with id: " + this._insertedId1);
        }

        [Test, Order(3)]
        public async Task Test_02_Given_UpsertRequest_When_Upsert_Then_InsertOneRecord()
        {
            // test Upsert 会插入第二条记录
            EpisodeUpsertReq upsertReq = new EpisodeUpsertReq()
            {
                ID = this._insertedId2,
                Name = "测试音频02",
                ContentType = ContentType.COMEDY,
                FilterType = FilterType.RULED,
                Count = 2345,
                IsOnline = true,
            };
            EpisodeUpsertRes upsertRes = await this._episodeRepo.Upsert(upsertReq);
            Assert.IsNotNull(upsertRes);
            Assert.IsNotNull(upsertRes.UpsertedId);
            Assert.That(this._insertedId2, Is.EqualTo(upsertRes.UpsertedId));
            Assert.That(upsertRes.UpsertedCount, Is.EqualTo(1));
            Assert.That(upsertRes.MatchedCount, Is.EqualTo(0));
            Assert.That(upsertRes.ModifiedCount, Is.EqualTo(0));
            Console.WriteLine("Episode upserted with id: " + this._insertedId2);
        }

        [Test, Order(4)]
        public async Task Test_03_Given_OneRequest_When_Query_One_Then_ReturnOneRecord()
        {
            // test One 第1次，命中1条记录
            EpisodeOneReq oneReq = new EpisodeOneReq
            {
                Name = new Condition {operatorType = OperatorType.EQ, value = "测试音频01"},
                ContentType = new Condition {operatorType = OperatorType.EQ, value = (int) ContentType.SPORTS},
                IsOnline = new Condition {multi = MultiType.IN, value = new[] {false, true}},
                CreatedAt = new Condition {operatorType = OperatorType.GTE, value = this._dateStarted}
            };
            EpisodeOneRes oneRes = await this._episodeRepo.One(oneReq);
            Assert.IsNotNull(oneRes);
            Assert.IsTrue(oneRes.Find);
            Assert.That(oneRes.Item.Count, Is.EqualTo(1234));
            Assert.That(oneRes.Item.Keywords.Count, Is.EqualTo(3));
            Assert.That(oneRes.Item.Keywords[2], Is.EqualTo("关键词3"));
            Assert.That(oneRes.Item.Outlines.Count, Is.EqualTo(2));
            Assert.That(oneRes.Item.Outlines["介绍"], Is.EqualTo("介绍部分"));
            Assert.That(oneRes.Item.QAs.Count, Is.EqualTo(2));
            Assert.That(oneRes.Item.QAs[0].Question, Is.EqualTo("问题1"));
        }

        [Test, Order(5)]
        public async Task Test_04_Given_OneRequest_When_Query_One_Then_ReturnNoRecord()
        {
            // test One 第2次，无命中记录
            EpisodeOneReq oneReq = new EpisodeOneReq
            {
                Name = new Condition {operatorType = OperatorType.EQ, value = "测试音频01"},
                ContentType = new Condition {operatorType = OperatorType.EQ, value = (int) ContentType.COMEDY},
                IsOnline = new Condition {multi = MultiType.IN, value = new[] {false, true}},
                CreatedAt = new Condition {operatorType = OperatorType.GTE, value = this._dateStarted}
            };
            EpisodeOneRes oneRes = await this._episodeRepo.One(oneReq);
            Assert.IsNotNull(oneRes);
            Assert.IsFalse(oneRes.Find);
        }

        [Test, Order(6)]
        public async Task Test_05_Given_OneRequest_When_Query_One_Then_ReturnOneRecord()
        {
            // test One 第3次，命中1条记录
            EpisodeOneReq oneReq = new EpisodeOneReq
            {
                ID = new Condition {operatorType = OperatorType.EQ, value = this._insertedId1},
                Name = new Condition {operatorType = OperatorType.LIKE, value = "测试音频", wildcard = WildcardType.CONTAINS},
            };
            EpisodeOneRes oneRes = await this._episodeRepo.One(oneReq);
            Assert.IsNotNull(oneRes);
            Assert.IsTrue(oneRes.Find);
            Assert.That(oneRes.Item.Name, Is.EqualTo("测试音频01"));
        }

        [Test, Order(7)]
        public async Task Test_06_Given_CountRequest_When_Query_Count_Then_ReturnCountOfRecords()
        {
            // test Count 第1次，共2条满足条件的记录
            EpisodeCountReq countReq = new EpisodeCountReq
            {
                ContentType = new Condition {multi = MultiType.IN, value = new[] {(int) ContentType.SPORTS, (int) ContentType.COMEDY}},
                IsOnline = new Condition {multi = MultiType.IN, value = new[] {false, true}},
                CreatedAt = new Condition {operatorType = OperatorType.GTE, value = this._dateStarted}
            };
            EpisodeCountRes countRes = await this._episodeRepo.Count(countReq);
            Assert.IsNotNull(countRes);
            Assert.That(countRes.TotalElements, Is.EqualTo(2));
        }

        [Test, Order(8)]
        public async Task Test_07_Given_CountRequest_When_Query_Count_Then_ReturnCountOfRecords()
        {
            // test Count 第2次，共1条满足条件的记录
            EpisodeCountReq countReq = new EpisodeCountReq
            {
                Name = new Condition {operatorType = OperatorType.EQ, value = "测试音频01"},
                CreatedAt = new Condition {operatorType = OperatorType.GTE, value = this._dateStarted}
            };
            EpisodeCountRes countRes = await this._episodeRepo.Count(countReq);
            Assert.IsNotNull(countRes);
            Assert.That(countRes.TotalElements, Is.EqualTo(1));
        }

        [Test, Order(9)]
        public async Task Test_08_Given_CountRequest_When_Query_Count_Then_ReturnCountOfRecords()
        {
            // test Count 第3次，命中2条记录
            EpisodeCountReq countReq = new EpisodeCountReq
            {
                CreatedAt = new Condition {operatorType = OperatorType.GTE, value = this._dateStarted}
            };
            EpisodeCountRes countRes = await this._episodeRepo.Count(countReq);
            Assert.IsNotNull(countRes);
            Assert.That(countRes.TotalElements, Is.EqualTo(2));
        }

        [Test, Order(10)]
        public async Task Test_09_Given_ListRequest_When_Query_List_Then_ReturnList()
        {
            // test List 第1次，返回第2页，每页1条记录，当页有1条记录，为满足条件的第2条记录
            EpisodeListReq listReq = new EpisodeListReq
            {
                ContentType = new Condition {multi = MultiType.IN, value = new[] {(int) ContentType.SPORTS, (int) ContentType.COMEDY}},
                IsOnline = new Condition {multi = MultiType.IN, value = new[] {false, true}},
                CreatedAt = new Condition {operatorType = OperatorType.GTE, value = this._dateStarted},
                PageRequest = new PageRequest
                {
                    number = 2,
                    size = 1,
                    sorts = new[]
                    {
                        new SortParam
                        {
                            property = "Name",
                            direction = SortDirection.ASC
                        }
                    }
                }
            };
            EpisodeListRes listRes = await this._episodeRepo.List(listReq);
            Assert.IsNotNull(listRes);
            Assert.That(listRes.PageInfo.number, Is.EqualTo(2));
            Assert.That(listRes.PageInfo.totalElements, Is.EqualTo(2));
            Assert.That(listRes.PageInfo.totalPages, Is.EqualTo(2));
            Assert.That(listRes.PageInfo.numberOfElements, Is.EqualTo(1));
            Assert.That(listRes.PageInfo.first, Is.EqualTo(false));
            Assert.That(listRes.PageInfo.last, Is.EqualTo(true));
            Assert.That(listRes.Items.Count, Is.EqualTo(1));
            Assert.That(listRes.Items[0].Name, Is.EqualTo("测试音频02"));
        }

        [Test, Order(11)]
        public async Task Test_10_Given_ListRequest_When_Query_List_Then_ReturnRecordList()
        {
            // // test List 第2次，使用 extraFilters 查询数组元素，返回第1页的1条记录
            // EpisodeListReq listReq = new EpisodeListReq
            // {
            //     CreatedAt = new Condition {operatorType = OperatorType.GTE, value = this._dateStarted},
            //     Keywords = new Condition {operatorType = OperatorType.LIKE, value = "关键词", wildcard = WildcardType.STARTS_WITH},
            //     ExtraFilters = new List<PropertyFilter>
            //     {
            //         new PropertyFilter
            //         {
            //             property = "Keywords.1",
            //             condition = new Condition {operatorType = OperatorType.EQ, value = "关键词2"}
            //         }
            //     }
            // };
            // EpisodeListRes listRes = await this._episodeRepo.List(listReq);
            // Assert.IsNotNull(listRes);
            // Assert.That(listRes.PageInfo.number, Is.EqualTo(1));
            // Assert.That(listRes.PageInfo.totalElements, Is.EqualTo(1));
            // Assert.That(listRes.PageInfo.totalPages, Is.EqualTo(1));
            // Assert.That(listRes.PageInfo.numberOfElements, Is.EqualTo(1));
            // Assert.That(listRes.PageInfo.first, Is.EqualTo(true));
            // Assert.That(listRes.PageInfo.last, Is.EqualTo(true));
            // Assert.That(listRes.Items.Count, Is.EqualTo(1));
            // Assert.That(listRes.Items[0].Name, Is.EqualTo("测试音频01"));
            // Assert.That(listRes.Items[0].Keywords[0], Is.EqualTo("关键词01"));
            // Assert.That(listRes.Items[0].Keywords[1], Is.EqualTo("关键词02"));
        }

        [Test, Order(12)]
        public async Task Test_11_Given_ListRequest_When_Query_List_Then_ReturnRecordList()
        {
            // // test List 第3次，使用 extraFilters 查询 map<string, string>（存储为Object）的 key/value，返回第1页的1条记录
            // EpisodeListReq listReq = new EpisodeListReq
            // {
            //     CreatedAt = new Condition {operatorType = OperatorType.GTE, value = this._dateStarted},
            //     ExtraFilters = new List<PropertyFilter>
            //     {
            //         new PropertyFilter
            //         {
            //             property = "outlines.介绍",
            //             condition = new Condition {operatorType = OperatorType.EQ, value = "介绍部分"}
            //         }
            //     }
            // };
            // EpisodeListRes listRes = await this._episodeRepo.List(listReq);
            // Assert.IsNotNull(listRes);
            // Assert.That(listRes.PageInfo.number, Is.EqualTo(1));
            // Assert.That(listRes.PageInfo.totalElements, Is.EqualTo(1));
            // Assert.That(listRes.PageInfo.totalPages, Is.EqualTo(1));
            // Assert.That(listRes.PageInfo.numberOfElements, Is.EqualTo(1));
            // Assert.That(listRes.PageInfo.first, Is.EqualTo(true));
            // Assert.That(listRes.PageInfo.last, Is.EqualTo(true));
            // Assert.That(listRes.Items.Count, Is.EqualTo(1));
            // Assert.That(listRes.Items[0].Name, Is.EqualTo("测试音频01"));
            // Assert.That(listRes.Items[0].QAs[0].Question, Is.EqualTo("问题1"));
        }

        [Test, Order(13)]
        public async Task Test_12_Given_ListRequest_When_Query_List_Then_ReturnRecordList()
        {
            // // test List 第4次，使用 extraFilters 查询 []QuestionAnswer 数组的元素
            // EpisodeListReq listReq = new EpisodeListReq
            // {
            //     CreatedAt = new Condition {operatorType = OperatorType.GTE, value = this._dateStarted},
            //     ExtraFilters = new List<PropertyFilter>
            //     {
            //         new PropertyFilter
            //         {
            //             property = "QAs.0.Question",
            //             condition = new Condition {operatorType = OperatorType.LIKE, value = "问题1", wildcard = WildcardType.CONTAINS}
            //         }
            //     }
            // };
            // EpisodeListRes listRes = await this._episodeRepo.List(listReq);
            // Assert.IsNotNull(listRes);
            // Assert.That(listRes.PageInfo.number, Is.EqualTo(1));
            // Assert.That(listRes.PageInfo.totalElements, Is.EqualTo(1));
            // Assert.That(listRes.PageInfo.totalPages, Is.EqualTo(1));
            // Assert.That(listRes.PageInfo.numberOfElements, Is.EqualTo(1));
            // Assert.That(listRes.PageInfo.first, Is.EqualTo(true));
            // Assert.That(listRes.PageInfo.last, Is.EqualTo(true));
            // Assert.That(listRes.Items.Count, Is.EqualTo(1));
            // Assert.That(listRes.Items[0].Name, Is.EqualTo("测试音频01"));
            // Assert.That(listRes.Items[0].QAs[0].Question, Is.EqualTo("问题1"));
        }

        [Test, Order(14)]
        public async Task Test_13_Given_ListRequest_When_Query_List_Then_ReturnRecordList()
        {
            // // test List 第5次，使用 FieldsIncluded 返回特定字段
            // EpisodeListReq listReq = new EpisodeListReq
            // {
            //     CreatedAt = new Condition {operatorType = OperatorType.GTE, value = this._dateStarted},
            //     FieldsIncluded = new[] {"Name"}
            // };
            // EpisodeListRes listRes = await this._episodeRepo.List(listReq);
            // Assert.IsNotNull(listRes);
            // Assert.That(listRes.PageInfo.number, Is.EqualTo(1));
            // Assert.That(listRes.PageInfo.totalElements, Is.EqualTo(2));
            // Assert.That(listRes.PageInfo.totalPages, Is.EqualTo(1));
            // Assert.That(listRes.PageInfo.numberOfElements, Is.EqualTo(2));
            // Assert.That(listRes.Items.Count, Is.EqualTo(2));
            // Assert.That(listRes.Items[0].Name, Is.EqualTo("测试音频01"));
            // Assert.IsNotNull(listRes.Items[0].ID);
            // Assert.That(this._insertedId1, Is.EqualTo(listRes.Items[0].ID));
            // Assert.IsNull(listRes.Items[0].Keywords);
            // Assert.IsNull(listRes.Items[0].Outlines);
            // Assert.IsNull(listRes.Items[0].QAs);
        }

        [Test, Order(15)]
        public async Task Test_14_Given_GetRequest_When_Query_ID_Then_ReturnOneRecord()
        {
            // test Get 返回第一条记录
            EpisodeGetReq getReq = new EpisodeGetReq
            {
                ID = this._insertedId1
            };
            EpisodeGetRes getRes = await this._episodeRepo.Get(getReq);
            Assert.IsNotNull(getRes);
            Assert.IsNotNull(getRes.Name);
            Assert.That(getRes.Name, Is.EqualTo("测试音频01"));
        }

        [Test, Order(16)]
        public async Task Test_15_Given_UpdateRequest_When_Update_Then_UpdateOneRecord()
        {
            // test Update 修改第一条记录
            EpisodeUpdateReq updateReq = new EpisodeUpdateReq
            {
                ID = this._insertedId1,
                Name = "测试音频03",
                Count = 3456,
                IsOnline = false,
            };
            EpisodeUpdateRes updateRes = await this._episodeRepo.Update(updateReq);
            Assert.IsNotNull(updateRes);
            Assert.That(updateRes.ModifiedCount, Is.EqualTo(1));
            Assert.That(updateRes.MatchedCount, Is.EqualTo(1));
        }

        [Test, Order(17)]
        public async Task Test_16_Given_GetRequest_When_Query_ID_Then_ReturnOneRecord()
        {
            // test Get 再次验证第一条记录
            EpisodeGetReq getReq = new EpisodeGetReq
            {
                ID = this._insertedId1
            };
            EpisodeGetRes getRes = await this._episodeRepo.Get(getReq);
            Assert.IsNotNull(getRes);
            Assert.IsNotNull(getRes.Name);
            Assert.That(getRes.Name, Is.EqualTo("测试音频03"));
            Assert.That(getRes.Count, Is.EqualTo(3456));
            Assert.That(getRes.IsOnline, Is.EqualTo(false));
            this._record1CreatedAt = getRes.CreatedAt;
        }

        [Test, Order(18)]
        public async Task Test_17_Given_UpsertRequest_When_Upsert_Then_UpateOneRecord()
        {
            // test Upsert 修改第一条记录
            EpisodeUpsertReq upsertReq = new EpisodeUpsertReq()
            {
                ID = this._insertedId1,
                Name = "测试音频04",
                Count = 4567,
                IsOnline = true,
                // 设置 CreatedAt 的值，暗示 Upsert 操作实际会执行 Update
                CreatedAt = this._record1CreatedAt
            };
            EpisodeUpsertRes upsertRes = await this._episodeRepo.Upsert(upsertReq);
            Assert.IsNotNull(upsertRes);
            Assert.That(upsertRes.UpsertedCount, Is.EqualTo(0));
            Assert.That(upsertRes.MatchedCount, Is.EqualTo(1));
            Assert.That(upsertRes.ModifiedCount, Is.EqualTo(1));
        }

        [Test, Order(19)]
        public async Task Test_18_Given_GetRequest_When_Query_ID_Then_ReturnOneRecord()
        {
            // test Get 再次验证第一条记录
            EpisodeGetReq getReq = new EpisodeGetReq
            {
                ID = this._insertedId1
            };
            EpisodeGetRes getRes = await this._episodeRepo.Get(getReq);
            Assert.IsNotNull(getRes);
            Assert.IsNotNull(getRes.Name);
            Assert.That(getRes.Name, Is.EqualTo("测试音频04"));
            Assert.That(getRes.Count, Is.EqualTo(4567));
            Assert.That(getRes.IsOnline, Is.EqualTo(true));
            Assert.That(getRes.CreatedAt, Is.EqualTo(this._record1CreatedAt));
        }

        [Test, Order(20)]
        public async Task Test_19_Given_DeleteMultiRequest_When_DeleteMulti_Then_DeleteMultiRecord()
        {
            // test DeleteMulti 删除2条记录
            EpisodeDeleteMultiReq deleteMultiReq = new EpisodeDeleteMultiReq
            {
                CreatedAt = new Condition {operatorType = OperatorType.GTE, value = this._dateStarted},
            };
            EpisodeDeleteMultiRes deleteMultiRes = await this._episodeRepo.DeleteMulti(deleteMultiReq);
            Assert.IsNotNull(deleteMultiRes);
            Assert.That(deleteMultiRes.DeletedCount, Is.EqualTo(2));
        }

        [Test, Order(21)]
        public async Task Test_20_Given_DeleteRequest_When_Delete_Then_DeleteOneRecord()
        {
            // test Delete 删除0条记录，因为之前的 deleteMulti 已经删除过了
            EpisodeDeleteReq deleteReq = new EpisodeDeleteReq
            {
                ID = this._insertedId1
            };
            EpisodeDeleteRes deleteRes = await this._episodeRepo.Delete(deleteReq);
            Assert.IsNotNull(deleteRes);
            Assert.That(deleteRes.DeletedCount, Is.EqualTo(0));
        }
    }
}