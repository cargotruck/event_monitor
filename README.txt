/* event_monitor.exe
 * Created by nicholas.flesch@outlook.com
 * Last Modified: 2020.10.13
 * Description:
 *      The program reads a list of user defined Windows events,
 *      compares them with events logged in Windows Event Logs,
 *      and send an email alert to a specified email address through a 
 *      specificed email server when occurances of the user defined
 *      events are found in the Windows Event Logs.
 *      
 *      User defined events are categorized as 'o', one-off, or 
 *      'c', cluster. One-off events trigger the email alert upon
 *      their first occurance. Cluster events only trigger the 
 *      email alert if the event occurs a user defined number of times
 *      within a user defined time frame.
 *      
 *      Users can also whitelist event IDs and executables. When the
 *      executable is whitelisted the program will search the Win_event
 *      Message look for a string that matches the user defined executable.
 * 
 * Resources used:
 *  https://orangematter.solarwinds.com/2018/01/25/microsoft-workstation-logs-focus-on-whats-important/
 *  https://statemigration.com/top-3-workstation-logs-to-monitor/
*/
