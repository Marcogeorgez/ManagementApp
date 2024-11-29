Reason: when saving the note as soon as it opens (before it finishes rendering),
it saves it as a new note instead of checking if it exists first 
(this bug only exists on server cause it takes 1 second to render so it won’t 
show-up on my pc.


	























----------------------------
# Done This Week 20/11:
##### Implemented WYSIWYG HTML Editor checklist ✅ along with uploading images.
		(Note images will cost on hosting the database because of their size)

##### Implemented better editing on grid table admin dashboard.
##### Implemented editing on projects.

##### Client/Primary Editor/Secondary Editor are now visible on grid. 

##### Implemented grouping category
##### Implemented total count for each category
##### Implemented filtering on category (for later to be able to generate invoice)

##### Implemented hiding columns of the grid as requested.
##### Implemented basic swapping columns for the grid.


##### When clicking "Enter" or leaving the button of Usd to Lek it will save instantly 
		otherwise it will save after a delay of 2 second.

##### Many fixes and improving performance of project update operations.
##### Improved buttons to use numeric when possible
##### Implemented Calculations formula
##### Implemented Client Guidelines only visible on the client profile page.
##### Changed create project to fit as requested (from client side point of view).
##### created a warning symbol and added missing
		items and what are they when the client create a project.

##### Implemented different side-bar depending on the UI if mobile or not.

## Important need input:
1-Currently when the project is assigned to a new user,
It updates the shoot date by the weeks to due date default from
THE DATE OF SHOOT DATE.
example:
shoot date is 1 January 2025.
today is 8 january 2025.
if the client has assume 2 weeks of due date default.
new due date when assigned to client will be 1 January + 2 weeks = 15 January
is this correct functionality you want?

2-can editors assign project to client? (currently it is not since they can't even see all the projects unless they are assigned to that project.)



----------------------------

### Currently working on:


### Important Notes:
	Want an invoice which "Categorize" projects into a single category, and have an invoice 
		when the client pays the invoice, we mark that invoice as PAID.
			If the invoice is PAID all projects inside that invoice is marked as paid automatically
				Am I getting this correctly?
	Have another status for admin where it can be 
		"Not finished" "Delivered Not Paid" "Payment added to Main Editor" "Sent Invoice"
		"Paid"


----------------------------

### TODO later:
##### restructure db context to db context factory (time consuming for now)
##### -Bugs:



----------------------------------
# Done 1st Week
*Adding secure setting for the access to the web app.

*Implemented the database as we've discussed (will re-iterate it later as we need)

*Created roles (Admin-Editor-Client-Guest) and made sure every new user is just a guest.

*Created the admin dashboard where we can change a user role.

--------------------------------
# Done 2nd Week
#### -Responsive Website
#### -Made sure to update font to use inter font
#### -Implemented Notes in Database, so every admin user have his own private notes.
#### -Improved Admin dashboard layout:
	Done:
		Added a full-screen button for notes to enlarge it if you want to use it.
		Filtering Mode
		Notification when changing a user Role.
		Confirmation Dialogue when CHANGING ANY USER TO ADMIN FOR SAFETY.
		Toaster right bottom when the notes are saved.
		Saving the notes after .1 sec of stopping writing which removed the need for & save button (I think it is better for user experience this way, what do you think?)
		Better looking grid with easily readable data.
		Notes are specific for user so if there are 2 admins for example, they can't see each other notes about others.

		Q1: Case Insensitive in filtering is currently ignored, would you like to enable it?
		Q2: Currently guest user can be changed to client/editor one by one, would you like a button for changing multiple at same time?
			if so, where would you like the button be?
		Q3: Would you like a button to delete users from the database? (like removing his entire account?)
		Q4: Would you like the selected role (the new role that will be set to) as a drop menu as it is currently implemented or something else?
		

#### -Implemented Dark Mode storage in cookies so the user doesn't have to change it when he refreshes the page Improving UX.
#### -Implemented Login With Google on the sidebar with google logo icon since there was no needed for a separate page.
#### -Implemented Projects page:
		Done:
			Where every user can see his own related projects only and editors will be able to see projects assigned to them.
			Implemented add projects button, where it opens a window to add project information ( will modify as needed later)
			Due date is 4 weeks after shoot date (as you said in our first meeting or is that wrong? )
			Added an Expand button for description to show in a full-screen mode
			Added basic view for admin/editor/client
					(Where everyone has his own things to see according to the access sheet you gave to me)
			Added dynamic name greeting (will take the name from google account when user signs-in for first time but for now I'm displaying his email)
		
		Q1:The client be the one who create the project? or only the admin can create and give access to the client? or how would you like it.
		Q2:Shoot Date and Due date will start from today right?  so it can't accept any date of older than today or should I let it free to add dates
			(for example if you'd like to migrate old projects from google sheets to the website.)
		Q3: Should projects delete-able? or it can only be archieved?
		Q4: just to make sure, Only the admin can assign editors to the project right?
		
#### -(semi-finished) Implemented a denied access page.

#### Minor Changes:
###### -Implemented the scroll bar
###### -Icon depending on theme mode (Q:Do you like it as it is currently implemented?)
###### -Icons(arrows) for the open-close side bar
###### -Added Project button on the sidebar with this current icon, does it fit or would you like to change it?
###### -Added a loading indicator for tables when there are too many users or internet is slow to load data from server when deployed.
-------------------------------------------------------------
# Done 3rd week:
###### Implemented WYSIWYG HTML Editor (quill)
###### Implemented better editing on grid table admin dashboard with two modes
		-First mode is FORM which has better responsive UI on mobile.
		-Second mode is Cell, which has worse UI on mobile (more confusing)
		which do you prefer?
		should I remove button to change the user role since
			the form is better or keep both?

	Note: currently notes can't be saved from the form because 
			it is a bit complicated (html editor)
	Note: if I implement HTML Editor as it is, the notes won't be readable as text
			till you open the expand button.

###### Implemented two currency ($ and lek ) and a toggle to change the currency
		-Stores the currency on local storage of user device.
		 Currently, the values can be updated when changing $ value only not lek.
			Would you like to be able to change it using lek too ?

##### Implemented new dialog options, enum, progress bar, client only creating upcoming/
##### Implemented cusotm number of days to due date for each client in admin dashboard  
		#IMPORTANT WHEN ASSIGNING THE PROJECT TO ANOTHER CLIENT, IT TAKES THE CLIENT DUE DATE Weeks.
