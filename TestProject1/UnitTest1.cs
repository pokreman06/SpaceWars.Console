namespace TestProject1
{
    
    public class UnitTest1
    {
        [Fact]
        public void findNearestPlayerReturnsBlank()
        {
            var test = new Logic();
           Assert.Equal(test.findNearestPlayer(new (int,int)[] {}, 0), (0,0));
        }
    }
}