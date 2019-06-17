using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BlockHelper
{
    City city;
    public List<Block> newBlocks;
    List<Block> changedBlocks;
    GlobalNeeds globalNeeds;
    LandUseType _industrial;
    LandUseType _commercial;
    LandUseType _residential;

    public BlockHelper(City city, LandUseType industrial, LandUseType commercial, LandUseType residential)
    {
        this.city = city;
        newBlocks = new List<Block>();
        changedBlocks = new List<Block>();
        globalNeeds = new GlobalNeeds(city.globalIndustrial, city.globalResidential, city.globalCommercial);
        _industrial = industrial;
        _commercial = commercial;
        _residential = residential;
    }

    public void FindBlocks(Node start, int maxSize, int minSize)
    {
        List<Node> list = new List<Node>();
        for (int i = 0; i < start.neighbours.Count; i++)
        {
            list = FindCycle(start, start.neighbours[i], start, maxSize);

            if (list.Count >= minSize && !city.IsExistingBlock(list))
            {
                Block block = new Block(list);
                city.AddBlock(block);
                newBlocks.Add(block);
            }
        }
    }
    
    public List<Node> FindCycle(Node start, Node current, Node previous, int depth)
    {
        List<Node> list = new List<Node>();
        //found
        if (current == start)
        {
            list.Add(current);
            return list;
        }
        //max depth not reached
        else if (depth > 0)
        {
            list.Add(current);
            List<Node> tmp1 = new List<Node>();
            for (int i = 0; i < current.neighbours.Count; i++)
            {
                List<Node> tmp2 = new List<Node>();
                if (current.neighbours[i] != previous)
                    tmp2.AddRange(FindCycle(start, current.neighbours[i], current, depth - 1));
                //no repeated nodes && smallest cycle
                if (!tmp2.Contains(current) && tmp2.Count != 0)
                {
                    if (tmp1.Count >= tmp2.Count || tmp1.Count == 0)
                    {
                        tmp1.Clear();
                        tmp1.AddRange(tmp2);
                    }
                }
            }
            list.AddRange(tmp1);
        }
        if (list.Contains(start))
            return list;
        else
        {
            list.Clear();
            return list;
        }
    }

    float CalculateLUV(string lut, Block block)
    {
        switch (lut)
        {
            case "industrial":
                return 0.7f * globalNeeds.GetGlobalNeed("industrial") + 0.3f * _industrial.CalculateLocalNeed(block);
            case "commercial":
                return 0.7f * globalNeeds.GetGlobalNeed("commercial") + 0.3f * _commercial.CalculateLocalNeed(block);
            default:
                return 0.7f * globalNeeds.GetGlobalNeed("residential") + 0.3f * _residential.CalculateLocalNeed(block);
        }
    }

    public void AssingnLUT(Block block)
    {

        //find land use values for each lut
        float i = CalculateLUV("industrial", block);
        float c = CalculateLUV("commercial", block);
        float r = CalculateLUV("residential", block);

        //choose lut for block
        float lut = Math.Max(Math.Max(i, c), r);

        if (lut == i)
        {
            block.lut = _industrial;
            block.lut.luv = i;
            globalNeeds.numInd++;
        }
        else if (lut == c)
        {
            block.lut = _commercial;
            block.lut.luv = c;
            globalNeeds.numCom++;
        }
        else
        {
            block.lut = _residential;
            block.lut.luv = r;
            globalNeeds.numRes++;
        }
        return;
    }

    public void ReevaluateLUT()
    {
        List<Block> candidates = new List<Block>();

        int numCandidates = Math.Min((int)((double)city.blocks.Count * 0.1), 10);

        while (candidates.Count != numCandidates)
        {//get candidates
            int index = UnityEngine.Random.Range(0, city.blocks.Count - 1);
            if (!candidates.Contains(city.blocks[index]) && city.blocks[index].lut.getName() != "core")
                candidates.Add(city.blocks[index]);
        }

        for (int j = 0; j < candidates.Count; j++)
        {
            //find land use values for each lut
            float i = CalculateLUV("industrial", candidates[j]);
            float c = CalculateLUV("commercial", candidates[j]);
            float r = CalculateLUV("residential", candidates[j]);

            float newLUV = Math.Max(Math.Max(i, c), r);
            LandUseType newLUT;

            if (newLUV == i)
                newLUT = new Industrial();
            else if (newLUV == c)
                newLUT = new Commercial();
            else
                newLUT = new Residential();


            if (candidates[j].lut.getName() != newLUT.getName())
            { // better land use type found

                float currentLUV = 0f;

                switch (candidates[j].lut.getName())
                {
                    case "industrial":
                        currentLUV = i;
                        break;
                    case "commercial":
                        currentLUV = c;
                        break;
                    default:
                        currentLUV = r;
                        break;
                }

                float diff = newLUV - currentLUV;

                //calculate cost of replacement
                float cost = 0.7f - diff;

                if (candidates[j].building.size == 1)
                    cost += 0.1f;
                else if (candidates[j].building.size == 2)
                    cost += 0.2f;
                else
                    cost += 0.3f;

                var random = UnityEngine.Random.Range(0, 100);
                if (random > cost * 100)
                {
                    //replace lut
                    ReplaceLUT(candidates[j], newLUT);
                    changedBlocks.Add(candidates[j]);
                }

            }
        }

    }

    void ReplaceLUT(Block block, LandUseType newLUT)
    {
        block.lut = newLUT;
        //update building
        if (!block.historicalCenter)
            block.ResetBlock();
        else
            block.UpdateBuilding();
        //reset traffic
        ResetTraffic(block);
        //add to new blocks
        newBlocks.Add(block);

    }

    public void RecalculateTrips()
    {
       for (int i = 0; i < changedBlocks.Count; i++)
        {
            ResetTraffic(changedBlocks[i]);
            FindTrips(changedBlocks[i]);
        }
        changedBlocks.Clear();
    }

    void ResetTraffic(Block block)
    {
        for (int j = 0; j < block.trips.Count; j++)
        {//iterate through trips in block
            for (int k = 0; k < block.trips[j].path.Count; k++)
            {//iterate through edges on trip
                block.trips[j].path[k].traffic -= block.trips[j].volume;
                block.trips[j].path[k].StreetCost();
            }
        }
    }

    public void FindTrips(Block block)
    {
        float distance = float.MaxValue;
        int foundTrips = 0;
        Node end = new Node(Vector3.up);
        List<Block> tmpList = new List<Block>(city.blocks);

        //tmpList = city.blocks;
        tmpList.Remove(block);

        //Dijkstra
        Traffic.FindDistances(block.nodes[0], city.streetGraph.corners);

        //choose trip destinations
        int tripNumber = Math.Min(city.blocks.Count - 1, 10);

        if (tripNumber == city.blocks.Count - 1)
        {
            for (int i = 0; i < tmpList.Count; i++)
            {
                distance = float.MaxValue;
                //calculate trip attractiveness
                for (int j = 0; j < tmpList[i].nodes.Count; j++)
                {
                    if (tmpList[i].nodes[j].tDistance < distance)
                    {
                        distance = tmpList[i].nodes[j].tDistance;
                        end = tmpList[i].nodes[j];
                    }
                }
                //find and add trip
                float attractiveness = block.lut.TripAttractiveness(distance, tmpList[i].lut.getName(), tmpList[i].numInhabitants);
                List<Edge> path = Traffic.GetPath(block.nodes[0], end);
                Trip trip = new Trip(path, distance, attractiveness);
                block.trips.Add(trip);
            }
        }
        else
        {
            while (foundTrips < tripNumber)
            {
                int dest = UnityEngine.Random.Range(0, tmpList.Count - 1);
                //calculate trip attractiveness
                for (int i = 0; i < tmpList[dest].nodes.Count; i++)
                {
                    if (tmpList[dest].nodes[i].tDistance < distance)
                    {
                        distance = tmpList[dest].nodes[i].tDistance;
                        end = tmpList[dest].nodes[i];
                    }
                }

                float attractiveness = block.lut.TripAttractiveness(distance, tmpList[dest].lut.getName(), tmpList[dest].numInhabitants);
                if (UnityEngine.Random.Range(0f, 1f) <= attractiveness)
                {
                    //find and add trip
                    List<Edge> path = Traffic.GetPath(block.nodes[0], end);
                    Trip trip = new Trip(path, distance, attractiveness);
                    block.trips.Add(trip);

                    foundTrips++;
                    tmpList.RemoveAt(dest);
                }
            }
        }
    }

    public void UpdateTraffic(Block block)
    {
        float aSum = 0f; //sum of attractions

        for (int i = 0; i < block.trips.Count; i++)
            aSum += block.trips[i].attractiveness;

        //aSum = Math.Max(aSum, 1);
        int tripNum = block.numInhabitants * 2;

        for (int j = 0; j < block.trips.Count; j++)
        {//iterate through trips in block
            block.trips[j].volume = (int)((float)tripNum * block.trips[j].attractiveness / aSum) - block.trips[j].volume;

            for (int k = 0; k < block.trips[j].path.Count; k++)
            {//iterate through edges on trip
                AddStreetTraffic(block.trips[j].path[k], block.trips[j].volume);
                block.trips[j].path[k].StreetCost();
            }
        }

    }

    public void AddStreetTraffic(Edge street, int traffic)
    {
        var newTraffic = Math.Max(street.traffic + traffic, 0);
        if (newTraffic <= street.capacity)
        {
            street.traffic = newTraffic;
            return;
        }
        else
        {
            street.traffic = street.capacity;
            street.Grow(); 
        }
    }

    public void UpdateInhabitants(Block block)
    {
        int tmp = block.lut.InhabitantGrowth(block, block.numInhabitants);

        if (tmp != block.numInhabitants && tmp < block.inhabitantCapacity)
        {
            block.numInhabitants = tmp;
            changedBlocks.Add(block);
        }
        else if(!block.historicalCenter)
        {    
            //chance to expand building
            if (block.building.size < 3 && UnityEngine.Random.Range(0f, 1f) > block.lut.luv)
            {
                block.building.GrowBuilding();
                return;//expand building
            }
 
        }
    }
}
