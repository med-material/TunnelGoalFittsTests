//Output variables
int EDA = 0;              // Raw EDA measurement
int IBI = 600;            // Time interval between heart beats
int pressure = 0;         // Raw Pressure Sensor Measurement
int rawPulseSensor = 0;   // Raw reading from the pulse sensor pin (can be used for plotting the pulse)

//Sensor Variables
int EDAPin = A0;
int PulsePin = A1;         // Pulse Sensor purple wire connected to analog pin 0
int pressurePin = A2;
int buttonPin = 2;

int IBISign = 1;           // used to change the sign of the IBI, to indicate a new value (in case two succeeding values are the same)
boolean Pulse = false;     // "True" when a heartbeat is detected. (can be used for for blinking an LED everytime a heartbeat is detected) 
bool NewPulse = false;

//Serial variables
bool Connected = false;
const unsigned int serialOutputInterval = 10; // Output Frequency = 1000 / serialOutputInterval = 1000 / 10 = 100Hz
unsigned long serialLastOutput = 0;
const char StartFlag = '#';
const String Delimiter = "\t";

void setup() {
  Serial.begin(115200);

//Example input/output pins
  pinMode(13, OUTPUT);
  pinMode(12, OUTPUT);
  pinMode(11, OUTPUT);
  pinMode(10, OUTPUT);
  pinMode(9, OUTPUT);
  
  pinMode(A0, INPUT);
  pinMode(A1, INPUT);
  pinMode(A2, INPUT);
  pinMode(2, INPUT);

  analogReference(EXTERNAL);
}

void loop() {

  ReadSensors(); //Have the Arduino read it's sensors etc.

  SerialInput(); //Check if Unity has sent anything

  SerialOutput(); //Check if it is time to send data to Unity

  digitalWrite(13,Pulse); // Light up the LED on Pin 13 when a pulse is detected
}

void ReadSensors()
{
  EDA = analogRead(EDAPin); //Read the raw 

  pressure = analogRead(pressurePin);

  checkPulseSensor();
}

const int inputCount = 4; //This must match the amount of bytes you send from Unity!
byte inputBuffer[inputCount];
void SerialInput()
{  
  if(Serial.available() > 0){ //check if there is some data from Unity
     Serial.readBytes(inputBuffer, inputCount); //read the data
     //Use the data for something
     digitalWrite(13, inputBuffer[0]);

     analogWrite(3, inputBuffer[1]);
     analogWrite(5, inputBuffer[2]);
     analogWrite(6, inputBuffer[3]);


     //You could for example use the data for playing patterns
     //e.g. first value indicates which player and second value indicates which pattern to play
  }

     //Currently no checks for desync (no start/end flags or package size checks)
     //This should be implemented to make the imp. more robust
}



void SerialOutput() {
  //Time to output new data?
  if(millis() - serialLastOutput < serialOutputInterval)
    return;
  serialLastOutput = millis();


  //Write data package to Unity
  Serial.write(StartFlag);    //Flag to indicate start of data package
  Serial.print(millis());     //Write the current "time"
  Serial.print(Delimiter);    //Delimiter used to split values
  Serial.print(EDA);          //Write a value
  Serial.print(Delimiter);    //Write delimiter

  if(NewPulse){
    Serial.print(IBI);        //Only print IBI if a new pulse has been detected
    NewPulse = false;
  } else {
    Serial.print(0);          //else print 0
  }
  
  Serial.print(Delimiter);
  Serial.print(rawPulseSensor);
  Serial.print(Delimiter);
  Serial.print(pressure);
  Serial.print(Delimiter);
  Serial.print(digitalRead(buttonPin));
  Serial.println();           // Write endflag '\n' to indicate end of package

  //For debugging. Comment the lines above and uncomment one of these.
  //Serial.println(analogRead(EDAPin));
  //Serial.println(analogRead(PulsePin));
}
