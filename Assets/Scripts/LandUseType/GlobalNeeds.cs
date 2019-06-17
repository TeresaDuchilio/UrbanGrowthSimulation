using System.Collections.Generic;

public class GlobalNeeds {
	float industrial;
	float residential;
	float commercial;

	public int numRes;
	public int numInd;
	public int numCom;

	public GlobalNeeds(float industrial, float residential, float commercial){
		numCom = 0;
		numInd = 0;
		numRes = 0;

		this.industrial = industrial;
		this.commercial = commercial;
		this.residential = residential;
	}


	public float GetGlobalNeed(string lut){
		float numBlocks = numCom + numInd + numRes; 
		if (numBlocks == 0)
			numBlocks = 1;

		switch (lut) {
		case "residential": 
			return residential - numRes / numBlocks + (numCom + numInd) / numBlocks;
		case "industrial": 
			return industrial - numInd / numBlocks + (numCom + numRes) / numBlocks;
		default: 
			return commercial - numCom / numBlocks + (numRes + numInd) / numBlocks;
		}

	}
}
