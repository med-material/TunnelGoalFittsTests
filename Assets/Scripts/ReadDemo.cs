using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 
public class ReadDemo : MonoBehaviour {
 
	public TextAsset ConfigFile; 
	public string[,] configuration;

	public void Read (){
		CSVReader.DebugOutputGrid( CSVReader.SplitCsvGrid(ConfigFile.text) ); 

		//Debug.Log(configuration[2,2]);
	}
}