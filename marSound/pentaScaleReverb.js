inlets = 3;
outlets = 1;

var penta = [0, 2, 4, 7, 9];
var major = [0, 2, 4, 5, 7, 9, 11];
var minor = [0, 2, 3, 5, 7, 9, 10];
var transp = [0, 12];
var groundNote = 60;
var scaleSwitch = 0;

function msg_int(num){
	if (inlet == 1){
		//change groundNote
		groundNote = num;
		}
	else if (inlet == 2){
		//switch for scales
		scaleSwitch = num;
		}
	}
	
function bang(){
	//pick an octave
	var multiplier = transp[Math.round(Math.random()*(transp.length-1))];
	
	//choose pentatonic
	if (scaleSwitch == 0){
		var listIndex = Math.round(Math.random()*(penta.length-1));
		var outNote = groundNote + penta[listIndex] + multiplier;
		}
	//choose major
	else if (scaleSwitch == 1){
		var listIndex = Math.round(Math.random()*(major.length-1));
		var outNote = groundNote + major[listIndex] + multiplier;
		}
	//choose minor
	else if (scaleSwitch == 2){
		var listIndex = Math.round(Math.random()*(minor.length-1));
		var outNote = groundNote + minor[listIndex] + multiplier;
		}
	//send note to max/msp
	outlet(0, outNote);
	}