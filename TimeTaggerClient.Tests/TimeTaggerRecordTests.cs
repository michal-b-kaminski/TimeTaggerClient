using NUnit.Framework;

using TimeTaggerClient.Models;

namespace TimeTaggerConverter.Tests
{
    public class TimeTaggerRecordTests
    {
        // TODO implement test for Settings
        // TODO implement tests for TimeTaggerClient (consider how to mock API server; test fetch records, fetch settings, fetch empty records, fetch empty settings, fetch new with and without reset, invalid parameters - exceptions)

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void DescriptionDefaultValue()
        {
            var record = new TimeTaggerRecord(Guid.NewGuid().ToString(), DateTime.Now.AddMinutes(-5), DateTime.Now);
            Assert.That(record.Description, Is.SameAs(string.Empty));
        }

        [Test]
        public void DescriptionExplicitValue()
        {
            string description = "test #dots #code";
            var record = new TimeTaggerRecord(Guid.NewGuid().ToString(), DateTime.Now.AddMinutes(-5), DateTime.Now, Description: description);
            Assert.That(record.Description, Is.SameAs(description));
        }

        [Test]
        public void ModifiedTimeDefaultValue()
        {
            var record = new TimeTaggerRecord(Guid.NewGuid().ToString(), DateTime.Now.AddMinutes(-5), DateTime.Now );
            Assert.That(record.ModifiedTime, Is.EqualTo(DateTime.Now).Within(1).Seconds);
        }

        [Test]
        public void ModifiedTimeExplicitValue()
        {
            var modifiedTime = DateTime.Now.AddMinutes(-10);
            var record = new TimeTaggerRecord(Guid.NewGuid().ToString(), DateTime.Now.AddMinutes(-5), DateTime.Now, ModifiedTime: modifiedTime);
            Assert.That(record.ModifiedTime, Is.EqualTo(modifiedTime));
        }

        [Test]
        public void ServerTimeDefaultValue()
        {
            var record = new TimeTaggerRecord(Guid.NewGuid().ToString(), DateTime.Now.AddMinutes(-5), DateTime.Now );
            Assert.That(record.ServerTime, Is.EqualTo(DateTime.UnixEpoch));
        }

        [Test]
        public void ServerTimeExplicitValue()
        {
            var serverTime = DateTime.Now.AddMinutes(-10);
            var record = new TimeTaggerRecord(Guid.NewGuid().ToString(), DateTime.Now.AddMinutes(-5), DateTime.Now, ServerTime: serverTime);
            Assert.That(record.ServerTime, Is.EqualTo(serverTime));
        }

        [Test]
        public void DurationEnded()
        {
            var start = DateTime.Now.AddMinutes(-5);
            var end = DateTime.Now.AddMinutes(-1);
            var record = new TimeTaggerRecord(Guid.NewGuid().ToString(), start, end);
            Assert.That(record.Duration, Is.EqualTo(end - start));
        }

        [Test]
        public void DurationSEnded()
        {
            var start = DateTime.Now.AddMinutes(-5);
            var end = DateTime.Now.AddMinutes(-1);
            var record = new TimeTaggerRecord(Guid.NewGuid().ToString(), start, end);
            Assert.That(record.DurationS, Is.EqualTo(240));
        }

        [Test]
        public void DurationInProgress()
        {
            var start = DateTime.Now.AddMinutes( -5 );
            var end = DateTime.Now.AddMinutes( -5 );
            var record = new TimeTaggerRecord( Guid.NewGuid().ToString(), start, end );
            Assert.That( record.Duration, Is.EqualTo( DateTime.Now - start ).Within( 1 ).Seconds );
        }

        [Test]
        public void DurationSInProgress()
        {
            var start = DateTime.Now.AddMinutes( -5 );
            var end = DateTime.Now.AddMinutes( -5 );
            var record = new TimeTaggerRecord( Guid.NewGuid().ToString(), start, end );
            Assert.That( record.DurationS, Is.EqualTo( (DateTime.Now - start).TotalSeconds ).Within( 1 ) );
        }

        [Test]
        public void TagsEmpty()
        {                                    
            var description = "description without tags";
            var record = new TimeTaggerRecord( Guid.NewGuid().ToString(), DateTime.Now.AddMinutes(-5), DateTime.Now, description);
            Assert.That(record.Tags, Is.Empty);
        }

        [Test]
        public void TagsEmptyDescription()
        {                                    
            var description = string.Empty;
            var record = new TimeTaggerRecord( Guid.NewGuid().ToString(), DateTime.Now.AddMinutes( -5 ), DateTime.Now, description );
            Assert.That( record.Tags, Is.Empty );
        }

        [Test]
        public void TagsNullDescription()
        {
            var record = new TimeTaggerRecord( Guid.NewGuid().ToString(), DateTime.Now.AddMinutes( -5 ), DateTime.Now, null );
            Assert.That( record.Tags, Is.Empty );
        }

        [Test]
        public void TagsOneTagAtStart()
        {
            var description = "#dots test";
            var record = new TimeTaggerRecord( Guid.NewGuid().ToString(), DateTime.Now.AddMinutes( -5 ), DateTime.Now, description );
            var tags = record.Tags;
            Assert.That( tags, Has.Exactly( 1 ).EqualTo("#dots") );
            Assert.That( tags.Count(), Is.EqualTo( 1 ) );
        }

        [Test]
        public void TagsOneTagInMiddle()
        {
            var description = "test #dots test";
            var record = new TimeTaggerRecord( Guid.NewGuid().ToString(), DateTime.Now.AddMinutes( -5 ), DateTime.Now, description );
            var tags = record.Tags;
            Assert.That( tags, Has.Exactly( 1 ).EqualTo( "#dots" ) );
            Assert.That( tags.Count(), Is.EqualTo( 1 ) );
        }

        [Test]
        public void TagsOneTagAtEnd()
        {
            var description = "test #dots";
            var record = new TimeTaggerRecord( Guid.NewGuid().ToString(), DateTime.Now.AddMinutes( -5 ), DateTime.Now, description );
            var tags = record.Tags;
            Assert.That( tags, Has.Exactly( 1 ).EqualTo( "#dots" ) );
            Assert.That( tags.Count(), Is.EqualTo( 1 ) );
        }

        [Test]
        public void TagsTwoTags()
        {
            var description = "#dots #code test";
            var record = new TimeTaggerRecord( Guid.NewGuid().ToString(), DateTime.Now.AddMinutes( -5 ), DateTime.Now, description );
            var tags = record.Tags;
            Assert.That( tags, Has.Exactly( 1 ).EqualTo( "#dots" ) );
            Assert.That( tags, Has.Exactly( 1 ).EqualTo( "#code" ) );
            Assert.That( tags.Count(), Is.EqualTo( 2 ) );
        }
    }
}