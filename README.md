# ***1. Introduction***

	This document contains the development process for creating  APP TODO List for Embrace IT made as an intership exercise  in company.

	The requirements for this project are found in the request.pdf document provided by the  person in charge of the internship.

	With this document, it was agreed sending weekly email on the first day  to those responsible for the internships.

	This email will include the work sequence for that week, and on the last day of the week,  this sequence will be verified through repositories or  relevant documentation.

# ***2. Tutorial Followed***

	In my search of information, I found this tutorial wich explains all the concepts we need to implement in backend Structure : 
	- https://www.youtube.com/watch?v=RRrsFE6OXAQ&list=LL&index=3 


# *** Weekly Sprint (10/13)***

  
## *** Project Structure.***
  
  - Empty solution creation.
  - Projects creation as library classes:  Domain, Data, Application.
  - MinimalApi Project creation.

## *** Domain Layer.***

- Model folder created inside Domain Library Class containning entity models:
  
  - Task
  - Tag
  - Category
  - SubTask
  - TaskTag
  - TaskCategory
  - User (Deleted)

*Modified Models for Identity implementation.

## ***5. Data Layer.***

- AppDbContext file created inside Data layer.
- Configurations added :

```
  protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
```

- AppDbContext modified to heredate IdentityDbContext.

- Nuget Packages :
  - Microsoft.EntityFrameworkCore.SqlServer. 
  - Microsoft.EntityFrameworkCore.
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore.

- Repositories folder created containing implementation for repositories interfaces.
  - CategoryRepository
  - SubTaskRepository
  - TagRepository
  - taskRepository

	*Clases were modified to Identity implementation

- Configurations folder containing external entities configuration used in dbcontext :

  - CategoryConfiguration
  - SubTaskConfiguration
  - TagConfiguration
  - TaskConfiguration
  - UserConfiguration (Deleted)
  - TaskTagConfiguration
  - TaskCategoryConfiguration

	*Modified configurations to implement Identity User.

- Identity Folder added to contain Application user model.

- Migrations folder generated in this Layer for Data Base Migration.

- Abstractions/ITokenService.cs moved from Appplication project to solve Dependency Cycle.

- Services/TokenService.cs moved from Application Layer to solve dependency Cycle.


# ***6. Application Layer.***
	
- Abstractions Folder on Application Project Containning Repository Interfaces for Repository Pattern. (Moved To Domain Project to solve dependency Cycle)

- Identity Folder added to contain Application user model

- Migrations folder generated in this Layer for Data Base Migration.
  

## *** Application Layer.***
	
- Abstractions Folder on Application Project Containning Repository Interfaces for Repository Pattern.


  - ICategoryRepository
  - ISubTaskRepository
  - ITagRepository
  - ITaskRepository
  - IuserRepository (Deleted)
  - ITokenService (Moved)
 
  - Service/Token service implementation (Moved to Data Layer).

## *** Minimal API.***

  - This project is set as single startup project in Solution.
    
  - Repositories Dependency Injection register in Program.cs :

  ```
    builder.Services.AddScoped<ITaskRepository,TaskRepository>();
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped<ITagRepository, TagRepository>();
    builder.Services.AddScoped<ISubTaskRepository, SubTaskRepository>();
  ```

  - DbContext Dependency Injection register in Program.cs:
  ```
    builder.Services.AddDbContext<AppDbContext>();   
  ```

  - Dependency Injection for Identity implementation in Program.cs.
  
  - Connection String added on appsettings.json.

  - Packages installation:

    - Microsoft.EntityFrameworkCore.SqlServer.
    - Microsoft.EntityFrameworkCore.
    - Microsoft.EntityFrameworkCore.Tools

----

# *** Weekly Sprint (10/20)***

- JWT
- Solve DEPENDENCY CYCLE between Application and Data.
- Modificate Repositories for Linq Qeries addition.
- Middlewares.
- Minimal API Controllers Based.
- Start Angular Learning.


## ***JWT***

- Add configurations in appsetting.json.
- Interface ITokenService.cs creation in Data/Abstractions.
- implementing interface TokenService.cs in Data/Services.
- JWT configuration in Program.cs(Authentication, DI and Middleware)
- Auth endpoints and DTO's creation.
- Identity integration.


## ***Solving Dependency Cycle***

- Abstractions Folder on Application Project Containning Repository Interfaces for Repository Pattern Moved To Domain Project to solve dependency Cycle.

  - ICategoryRepository.
  - ISubTaskRepository.
  - ITagRepository.
  - ITaskRepository.
 
- Abstractions/ITokenService.cs moved from Appplication project to solve Dependency Cycle.

- Services/TokenService.cs moved from Application Layer to solve dependency Cycle.


## ***Linq Queries***

- All Repositories classes were modified to implement LINQ syntax queries

 
## ***Middleware***

- Middleware folder created in minimal api Layer.
- Created ExceptionHandlingMiddleware.cs for global exception handler.
- Created RequestLoginMiddleware to help in console debug.
- Middleware Registered in program.cs


## ***Minimal API Controller Based***

- DTO Folder Creation containning DTO files used in endpoints.
- Endpoints Folder Created to contain endpoint files.
- Created Minimal API Endpoints files using repositories and DTO.


## ***Documentation***

- Installed NuGet package Swashbuckle.AspNetCore to generate API documentation.
- Configurations Added in Program.cs
- added Endpoints annotations modifing with sintax methods(summary, tags...)
- Tried endpoints with Swagger help


  ## ***Linq Queries***

  - All Repositories classes were modified to implement LINQ syntax queries
    
 
  ## ***Middleware***

  - Middleware folder created in minimal api Layer.
  - Created ExceptionHandlingMiddleware.cs for global exception handler.
  - Created RequestLoginMiddleware to help in console debug.
  - Middleware Registered in program.cs

----

# ***Weekly Sprint (10/27)***

	- Start Conventional Commits Learning.
	
	- Initiate conventional commits structure on GitHub.
	
	- Services in clean code architecture researching to solve Dependency Cycle properly.
	
	- Solve DTO propper location folder to acomplish clean code architecture.
	
	- Solve dependency cycle properly by implementing services on Application Layer
	
	- Endpoints Testing.
	
	- Start Angular Topics Learning.
	
	- New project Deployment.
	
	- Build clean architecture Angular Project.
	
	- Dependencies instalation.
	
	- Environment Configuration.
	
	- Practice Angular Basics.


## ***Conventional Commits(and branches) Learning***

	- https://www.conventionalcommits.org/en/v1.0.0/
	
	- https://conventional-branch.github.io/#summary 
	
	- https://gist.github.com/vtenq/7a93687108cb876f884c3ce75a8a8023 


## ***Initiate conventional commits structure on GitHub***

 	- blank develop branch created.

	- Delete wrong named current branch bugfix-+-NextSprint.

	- Interactive commit rebase from root for correct naming.

	- Design organized branches structure and create on git:
	
		- Chore
		- bugfix
		- feature
	
	- Divide each branch in specific branches:
	
		- chore/: init , readme and code-organization 
		- bugfix/: database-connection, dependency-cycles and endpoint-errors
		- feature/: api-endpoints, identity-auth, middleware, repository pattern and swagger docs

	- main branch Cleaned.

	- Move commits into organized branches structure using cherry-pick.

	- Pull request develop branch based and resolving conflicts on GitHub.
	

	
## ***Services in clean code architecture researching to solve Dependency Cycle properly***

## ***Solve DTO propper location folder to acomplish clean code architecture***
 
## ***Solve dependency cycle properly by implementing services on Application Layer***
 
## ***Endpoints Testing***
 
## ***Start Angular Topics Learning***
 
## ***New project Deployment***
 
## ***Build clean architecture Angular Project***
 
## ***Dependencies instalation***
 
## ***Environment Configuration***
 
## ***Practice Angular Basics***

