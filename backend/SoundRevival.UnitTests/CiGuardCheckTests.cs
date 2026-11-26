namespace SoundRevival.UnitTests;

// Temporary: proves that a failing test turns the CI red. Never merged.
public class CiGuardCheckTests
{
    [Fact]
    public void Guard_DeliberateFailure_FailsTheRun() => Assert.Fail("deliberate failure to check the CI");
}
