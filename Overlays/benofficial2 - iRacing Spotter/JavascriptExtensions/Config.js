// Enable the Spotter that shows orange bars when the spotter is calling a car left/right 
const g_EnableSpotter = true;

// Distance from another car at which the Spotter's orange bars will show. Also used for showing Blind Spot Monitor.
const g_SpotterThreshold = 4.5;

// Height of the Spotter's orange bars (between 1-129)
const g_SpotterHeight = 129;

// Width of the Spotter's orange bars
const g_SpotterWidth = 12;

// Thickness of the Spotter's border (between 0-3)
const g_SpotterBorder = 3;

// Enable the Rejoin Helper that shows the gap with the next incomming car
const g_EnableRejoinHelper = true;

// Speed in Km/h below which the Rejoin Helper will trigger
const g_RejoinHelperMinSpeed = 35;

// Minimum gap in seconds at which the Rejoin Helper will show the 'Clear'
const g_RejoinHelperClear = 3.5;

// Minimum gap in seconds at which the Rejoin Helper will show the 'Care'
const g_RejoinHelperCare = 1.5;

// Enable the Blind Spot Monitor that shows a yellow warning sign when cars overlap (iRacing does not support left/right)
const g_EnableBlindSpotMonitor = false;