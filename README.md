# ABSoftware.Core
This project is the absolute foundation for all ABSoftware applications, it provides many central parts for all ABSoftware programs.

## What does it provide?

The ABSoftware Core provides **four main things** along with some other things on the side. In summary, the four main things are:

- **Components** - ABSoftware applications are split up into units called "components" in order to keep everything organised, this gives you the ability to "register" (load) and "unregister" components and read all components of a certain type.
- **Registry** - ABSoftware applications have their own "registry", like the Windows registry, where all of the configuration is stored - the registry is generally shared across all ABSoftware applications installed on the system!
- **Process Manager** - In order to ensure ABSoftware applications run well, they use an asyncronous queuing system. When something needs to be done, it is "queued" up to run, and the ABSoftware Core will go through and execute all queued up items asyncrnously.
- **Locale** - ABSoftware Core also provides a locale system to help provide multiple language support and easy text management.

Those have further details below. Here are some other side things that ABSoftware.Core provides that aren't the "main" features:

- **Logging System** - ABSoftware.Core provides a very important logging system in order to help developers identify problems.
- **ViewModel Additions** - Adds extra things to view models that help the make the four main things syncronize with ViewModels easily, such as a `RegistrySync` attribute that will automatically keep a property linked to a certain registry item.

### Components

#### What is a component?

It's hard to define exactly what a component is, the best way of explaining a component is with the GUI. 

If you look at the GUI, you have menu items at the top, you have all of the window buttons (close, maximize, minimize) and, depending on the application, there may a toolbox and much more.

All of these are components - *each* MenuItem is a *component*, and the system that handles the GUI will populate that part with all "MenuItemComponents".

#### How are components implemented?

Declaring a type of component is done by inheriting the `Component` class - where you have to provide a name, description and a way of checking if a given component matches another. As well as any other information that specific type of component needs (for example, in a MenuItem it needs the actual text that will display).

So, with the examples listed above, you'll get things such as `MenuItemComponent` and `WindowTitleButtonComponent`.

Now, if you want to actually add a new menu item, you'll create new instance of `MenuItemComponent`, which will have a constructor to provide all of the right things, and then you'll just call `ComponentManager.RegisterComponent(component)`, passing in that component.

Then, what the UI will actually be doing automatically is it will be running `ComponentManager.GetComponentOfType<MenuItemComponent>` to get all of the registered menu items (as well as taking advantage of the ComponentManager's event system for notifying when a new component is registered).

### Registry

The registry is quite simple, it's got a file-like structure, meaning you've got items and groups - and groups can contain sub-items and sub-groups.

Each of those items are what actually contain the values - they have a name and a value, there are three different types of items that all contain different values:

- **Boolean** - True/False value
- **Numerical** - A numerical value - stored as a `long`.
- **String** - Text

### Process Manager

The process manager is queue of different things that need to be executed, the process manager will go through this queue and execute each task asyncrnously.

There are three different priority level for processes that get queued up - high, medium and low. High priority tasks will always get executed first, if a lower priority task is already running, a higher priority task will be executed after that one has finished execution.

### Locale

**The locale system is still in development.**

## Documentation
There is no documentation at this moment in time, this will be updated when documentation has been written.
