# Spreadsheet-Animation-Events
A tool to get animation events from a Google spreadsheet/CSV file and apply the events directly to the animation clips. This tool was designed specifically for Nephilim and an example CSV has been included.

Buttons:
- Load From CSV: Will load data found from the CSV
- Write To CSV: Will write data from the scriptable object to the CSV file in "Text Asset Data" (Can not create a new text asset data/CSV file)
- Add FBXs to Scriptable Object: Below this button is a drop-down area where FBX animations can be dropped into. This will scan the animation clips found as well as their animation events and add them to the scriptable object
- Clear Object List: Removes all the objects in the aforementioned object list
- Apply CSV to FBX events: Will apply the data in the scriptable object to FBXs found in the Object list. It will ignore FBXs that don't have data in the scriptable object and vice versa

Steps:
1. Create the EventCSV scriptable object with the context menu shown below. This will house the logic and data of the CSV's animation events

![image](https://github.com/InsomniacSnorlax/Spreadsheet-Animation-Events/assets/94978222/285bc0f0-a00d-4116-9fb0-6b2a58c005fd)

2. Once created, select the scriptable object and input the CSV into "Text Asset Data"

![image](https://github.com/InsomniacSnorlax/Spreadsheet-Animation-Events/assets/94978222/f32dce4e-ef41-432b-96b9-ae091e795d97)

3. From there you can click Load From CSV to populate the data into the scriptable object.
4. Next is to drag FBXs with animation clips into the drop down
5. Lastly, click Apply CSV to FBX events to apply the animation events.

Details:
Headers: Headers are the animation event names found in the clips and the column names to the CSV format. This is only important when wanting to write the scriptable object's data into a CSV file to transfer to Google sheets.
Convert Header name will target the selected header and rename it to the input name in both the Header list as well as any animation event name of the same header name to the new name.

![image](https://github.com/InsomniacSnorlax/Spreadsheet-Animation-Events/assets/94978222/39cbe52c-16db-4b5e-b12f-32ed259ac099)
