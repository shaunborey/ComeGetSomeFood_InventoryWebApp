# Come Get Some Food - Inventory Web Application

The web application is an inventory management system for a fictional company which was created as a part of the application process for a potential employer.  The application allows the user to view current inventory, add new items, and edit existing items.  The administrator (and any other users belonging to the "Owner" role) is also able to delete items and add new users.  Additional functionality has also been implemented that will generate an order based upon current inventory levels and order thresholds configured by the user.  In a real-world implementation, this feature could be expanded to enable this order to be sent electronically to a supplier, but at this time the order is generated as a text file and is displayed for the user with the option to print.  The user can view the order history and close outstanding orders once the order items are received from the supplier, automatically updating inventory quantities of the order items.  The user may also close an order without updating the inventory (if the order is cancelled, for example). 

The web application also features an API that exposes a GetInventory() method with 2 overloads.  The first overload takes no arguments and will return all inventory items as a JSON string.  The second overload takes three integer arguments that compose a date (month, day, and year) and returns all inventory items that have been last received on or after the supplied date.  The method can be called by: 

```
/api/GetInventory  (all inventory)
/api/GetInventory/{month}/{day}/{year}  (all inventory received on or after date)
``` 

The inventory web application was designed in response to the following prompt:

> *Using ASP.NET and a SQL express or a LocalDB instance, you will create a web-based application to create and retrieve grocery information for a mock grocery store. The grocery store name is “Come Get Some Food” and the owner, Billy, has asked for an easy way for his employees to enter inventory so that he can keep track of it from home. He’ll need to know what the item is (description), SKU, brand, date received, quantity received and current quantity on hand. Please create a simple login mechanism (you can use a built-in library if you’d like), to limit 2 types of users: Owner and Employee. The Owner can Read/Write and Delete data but the Employee can only Read/Write (no delete).* 

> *“Corporate” requires that the store give access to inventory via a web service. Currently, they support REST or WCF, so it’s up to you. You’ll need to give them 1 API call to give them all inventory by date received and that should return a JSON object containing all DB information for that product. For time purposes, this web service API may be unsecured (no authentication).* 

> *You may use whichever flavor of ASP.NET you wish, along with any naming conventions or 3rd party libraries.* 

The open-ended nature of the prompt allowed me to showcase my own approach to solving the IT problem, establishing an identity through the structure of my work.  In order to develop a successful solution, I had to consider the problem from the perspective of the user to ensure maximum functionality balanced with optimum performance along with ease of use.  The application is built with a variety of front-end and back-end technologies including HTML, CSS, Javascript, JQuery, ASP.Net MVC with C#, and SQL Server database integration. 

Working on this project allowed me to be creative in my choice and use of technologies.  This creative process enabled me to gain a new perspective on the technologies with which I was already familiar by exploring new ways to integrate them into the project's purpose.

A video demo of this application can be viewed here: https://youtu.be/5hAg1sNGQ7I
 