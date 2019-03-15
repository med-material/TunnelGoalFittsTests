int Threshold = 525;                      // used to find instant moment of heart beat
int Peak = 0;                             // used to find peak in pulse wave
int Trough = 1023;                        // used to find trough in pulse wave
int Signal;                               // holds the incoming raw data
unsigned long lastBeatTime = 0;           // Store last millis a pulse was detected
unsigned long TimeoutInterval = 1500UL;
bool Resetting = true;

void checkPulseSensor(){
  Signal = analogRead(PulsePin);                  // Read the Pulse Sensor
  rawPulseSensor = Signal;
  unsigned long N = millis() - lastBeatTime;                // Calculate time since we last had a beat

  if (Signal < Threshold && Pulse == true && !Resetting) {      // When the values are going below the threshold, the beat is over
    Pulse = false;                                              // reset the Pulse flag so we are ready for another pulse
    Threshold = Trough + (Peak - Trough) * 0.75;                
    Peak = 0;                                                   // reset for next pulse
    Trough = 1023;                                              // reset for next pulse
    return;
  }
  
  //  Find the trough and the peak (aka. min and max) of the pulse wave (they are used to adjust threshold)
  if (Signal > Peak) {
    Peak = Signal;                           // keep track of highest point in pulse wave   
  }                                          
  if (Signal < Trough) {                     
    Trough = Signal;                         // keep track of lowest point in pulse wave
  }
    
  // Look for a heart beat
  if (N > 400) {                                            // avoid heart rates higher than 60000/400 = 150 bpm. Decrease the value to detect higher bpm
    if ( (Signal > Threshold) && !Pulse && !Resetting) {    // Signal surges up in value every time there is a pulse
      Pulse = true;                                         // set the Pulse flag when we think there is a pulse
      IBI = N;                                              // Set inter-beat interval
      lastBeatTime = millis();                              // keep track of time for next pulse
      NewPulse = true;                                      // Signal that a new pulse has been detected
      TimeoutInterval = 1500UL;                             // Reset the timeout variable
      return;
    }
  }



  if (N > TimeoutInterval) {                          // if 2.5 seconds go by without a beat, reset values and wait for a clear beat
    if(Resetting){
     Threshold = Trough + (Peak - Trough) * 0.75;
     Resetting = false;
    } else {
      Resetting = true;
      Threshold = 512;                       // set threshhold to default value
      Peak = 0;                            // set P default
      Trough = 1023;                          // set T default
      Pulse = false;
    }

    TimeoutInterval += 1500UL;
    
    //TimeoutInterval += 2500;
    //lastBeatTime = millis();               // bring the lastBeatTime up to date (would probably be better to just leave it be, and filter any large values (>2500) from the data)
  }
}
