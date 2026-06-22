namespace BizFlow.Schedules.Interval.Tests
{
    public class IntervalScheduleTests
    {
        [Fact]
        public void Constructor_ShouldThrow_WhenIntervalIsNegative()
        {
            Assert.Throws<ArgumentException>(() => new IntervalSchedule(TimeSpan.FromSeconds(-1)));
        }

        [Fact]
        public void Constructor_ShouldSetInterval_WhenValidIntervalProvided()
        {
            var interval = TimeSpan.FromSeconds(1);
            var scedule = new IntervalSchedule(interval);
            Assert.Equal(scedule.Interval, interval);
        }

        [Fact]
        public void GetNextRun_ShouldReturnNow_WhenLastRunIsNull()
        {
            var scedule = new IntervalSchedule(TimeSpan.FromHours(1));
            var now = new DateTimeOffset(2025, 5, 28, 12, 0, 0, TimeSpan.Zero);
            var next = scedule.GetNextRun(null, now);
            Assert.Equal(now, next);
        }

        [Fact]
        public void GetNextRun_ShouldReturnNow_WhenLastRunIsNull_WithDifferentOffset()
        {
            var scedule = new IntervalSchedule(TimeSpan.FromMinutes(10));
            var now = new DateTimeOffset(2025, 5, 28, 12, 0, 0, TimeSpan.FromHours(3));
            var next = scedule.GetNextRun(null, now);
            Assert.Equal(now, next);
            Assert.Equal(TimeSpan.FromHours(3), next!.Value.Offset);
        }

        [Fact]
        public void GetNextRun_ShouldReturnLastRunPlusInterval_WhenLastRunIsProvided()
        {
            var scedule = new IntervalSchedule(TimeSpan.FromDays(1));
            var lastRun = new DateTimeOffset(2025, 5, 28, 15, 0, 0, TimeSpan.Zero);
            var now = lastRun.AddHours(23);
            var next = scedule.GetNextRun(lastRun, now);
            Assert.Equal(next, lastRun.Add(TimeSpan.FromDays(1)));         
        }

        [Fact]
        public void GetNextRun_ShouldIgnoreNow_WhenLastRunIsProvided()
        {
            var schedule = new IntervalSchedule(TimeSpan.FromMinutes(5));
            var lastRun = new DateTimeOffset(2025, 5, 27, 10, 0, 0, TimeSpan.Zero);
            var now = lastRun.AddDays(10);
            var next = schedule.GetNextRun(lastRun, now);
            Assert.Equal(next, lastRun.Add(TimeSpan.FromMinutes(5)));
        }

        [Fact]
        public void GetNextRun_ShouldWorkWithFractionalIntervals()
        {
            var schedule = new IntervalSchedule(TimeSpan.FromMilliseconds(500));
            var lastRun = new DateTimeOffset(2025, 5, 27, 0, 0, 0, TimeSpan.Zero);
            var expected = lastRun.AddMilliseconds(500);
            var next = schedule.GetNextRun(lastRun, DateTimeOffset.UtcNow);
            Assert.Equal(next, expected);
        }

        [Fact]
        public void GetNextRun_ShouldNeverReturnNull()
        {
            var schedule = new IntervalSchedule(TimeSpan.FromSeconds(30));
            Assert.NotNull(schedule.GetNextRun(null, DateTimeOffset.UtcNow));
            Assert.NotNull(schedule.GetNextRun(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));
        }

        [Fact]
        public void GetNextRun_ShouldPreserveOffset_WhenAddingInterval()
        {
            var schedule = new IntervalSchedule(TimeSpan.FromHours(2));
            var lastRun = new DateTimeOffset(2025, 5, 27, 10, 0, 0, TimeSpan.FromHours(5));
            var next = schedule.GetNextRun(lastRun, DateTimeOffset.UtcNow);
            Assert.Equal(next!.Value.Offset, TimeSpan.FromHours(5));
        }
    }
}