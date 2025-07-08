# Red Mango Ecommerce API

This is the backend API for the **Red Mango** ecommerce food ordering application, built with **ASP.NET Core Web API** and **Entity Framework Core**. The API handles user authentication, menu item management, cart functionality, and order processing. It is deployed to **Azure** and uses **JWT** for secure access control.

## Authentication & Authorization

To test protected endpoints via Swagger UI, follow these steps:

1. **Login** with your credentials using the `/api/auth/login` endpoint.  
   Example login credentials (for testing):
   ```json
   {
     "userName": "admin@example.com",
     "password": "admin123*"
   } 
2. Copy the token value returned in the response. It will look something like: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
3. Click on the "Authorize" button in the top-right corner of Swagger UI.
4. In the input field, enter the token using this format: Bearer <paste-your-token-here>
5. Click "Authorize", and now you can access all secured endpoints.

   Test the API
You can test the live API here:
https://redmangoapinet.azurewebsites.net/index.html
