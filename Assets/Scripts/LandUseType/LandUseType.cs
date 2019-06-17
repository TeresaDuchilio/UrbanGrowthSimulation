public abstract class LandUseType{
	public float localNeed;
	public float luv;
    public float centreSize = 10;
	protected string name;

	abstract public float CalculateLocalNeed(Block block);
	abstract public int InhabitantGrowth (Block block, int currentInhabitants);
	abstract public float TripAttractiveness (float distance, string lut, int traffic);

	public string getName(){
		return name;
	}

}
