using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class City: MonoBehaviour {
	public StreetGraph streetGraph;
	public List<Block> blocks;
    public bool european;
    public bool trafficMode;
	EarlyGrowth earlyGrowth;
    LaterGrowth laterGrowth;
    Growth growth;
    int grow;

	[Range(5, 15)]
	public int streetLength;
	[Range(80, 100)]
	public int streetDegree;
	[Range(0, 1)]
	public float globalIndustrial;
	[Range(0, 1)]
	public float globalResidential;
	[Range(0, 1)]
	public float globalCommercial;

	void Start(){
		grow = 0;
		blocks = new List<Block>();
		streetGraph = new StreetGraph();
		earlyGrowth = new EarlyGrowth ();
        laterGrowth = new LaterGrowth();
        growth = new Growth();		
	}

	public void AddBlock(Block block){
		//add new block
		blocks.Add(block);
	}

	public bool IsExistingBlock(List<Node> nodes){
		bool tmp = true;
		for(int i=0; i<blocks.Count;i++){
			for(int j=0;j<nodes.Count;j++){
				if(!blocks[i].nodes.Contains(nodes[j]))
					tmp = false;
			}
			if(tmp)
				return tmp;
			tmp = true;
		}
		return false;

	}

	public void Expand(){
        if (european)
        {
            if (grow == 0)
            {
                earlyGrowth.SetTraffic(trafficMode);
                earlyGrowth.Initialize();                
            }
            else if (grow < 5)
            {
                earlyGrowth.SetTraffic(trafficMode);
                StartCoroutine(earlyGrowth.Grow());
                grow++;
            }
            else
            {
                laterGrowth.SetTraffic(trafficMode);
                StartCoroutine(laterGrowth.Grow());
            }
        }
        else
        {
            if (grow == 0)
            {
                growth.SetTraffic(trafficMode);
                growth.Initialize();
            }
            growth.SetTraffic(trafficMode);
            StartCoroutine(growth.Grow());
        }

        grow++;
	}

}
