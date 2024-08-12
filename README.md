
  

# Event-Driven Microservices Application

  

This project is an example of an event-driven application with microservices, developed using .NET 8 and Docker. The application is designed to handle user creation events and order processing, with RabbitMQ managing the event messaging between services.

  

## Overview

  

The application consists of three core microservices:

  

1.  **UserService**: Handles user creation and management.

2.  **OrderService**: Manages order creation and processing.

3.  **NotificationService**: Sends notifications and manages user and order events.

  

### Event-Driven Architecture

  

-  **User Creation Event**: When a user is created via the `UserService`, an event (`user.created`) is published to RabbitMQ. This event is consumed by both the `NotificationService` and the `OrderService`.

-  **NotificationService**: The `NotificationService` subscribes to the `user.created` event. When it receives this event, it stores the user's information and sends a welcome email to the user.

-  **OrderService**: The `OrderService` also subscribes to the `user.created` event. It stores the user ID in its own database to ensure referential integrity when an order is made.

  

-  **Order Creation Event**: When an order is created via the `OrderService`, an event (`order.created`) is published to RabbitMQ. The `NotificationService` subscribes to this event and sends an email with the order details to the user.

  

### Microservices

  

Each microservice is independently deployable and runs in its own Docker container. The services communicate with each other asynchronously through RabbitMQ.

  

-  **Database**: PostgreSQL is used as the database for all three services, with separate databases for each service.

-  **RabbitMQ**: Manages the messaging between services, ensuring that events are properly routed to the subscribing services.

-  **SMTP4Dev**: Used for testing email notifications sent by the `NotificationService`.

  

## Docker Setup

  

The project uses Docker Compose to define and run multi-container Docker applications. The `docker-compose.yml` file defines the services and their configurations.

  

```yaml

version: '3.8'

  

services:

db:

image: postgres:15

environment:

POSTGRES_USER: postgres

POSTGRES_PASSWORD: pass123~

POSTGRES_MULTIPLE_DATABASES: event-driven-user-service,event-driven-order-service,event-driven-notification-service

ports:

- "5432:5432"

volumes:

- ./create-multiple-postgresql-databases.sh:/docker-entrypoint-initdb.d/create-multiple-postgresql-databases.sh

  

rabbitmq:

image: rabbitmq:3-management

ports:

- "5672:5672"

- "15672:15672"

environment:

RABBITMQ_DEFAULT_USER: ruser

RABBITMQ_DEFAULT_PASS: pass123~

healthcheck:

test: ["CMD", "rabbitmq-diagnostics", "status"]

interval: 10s

timeout: 10s

retries: 5

  

smtp4dev:

image: rnwood/smtp4dev

ports:

- "3000:80"

- "2525:25"

environment:

ASPNETCORE_ENVIRONMENT: Development

MessageRetentionPeriod: "30.00:00:00"

MessageCountLimit: "100"

  

userserviceapi:

build:

context: .

dockerfile: UserService.Api/Dockerfile

ports:

- "5001:8080"

depends_on:

rabbitmq:

condition: service_healthy

db:

condition: service_started

environment:

ConnectionStrings__DefaultConnection: "Host=db;Database=event-driven-user-service;Username=postgres;Password=pass123~"

RabbitMQ__HostName: "rabbitmq"

RabbitMQ__UserName: "ruser"

RabbitMQ__Password: "pass123~"

RabbitMQ__UserPublishQueue__Queue: "owner.user.created"

RabbitMQ__UserPublishQueue__ExchangeName: "UserExchange"

RabbitMQ__UserPublishQueue__RoutingKey: "user.created"

  

orderserviceapi:

build:

context: .

dockerfile: OrderService.Api/Dockerfile

ports:

- "5002:8080"

depends_on:

rabbitmq:

condition: service_healthy

db:

condition: service_started

environment:

ConnectionStrings__DefaultConnection: "Host=db;Database=event-driven-order-service;Username=postgres;Password=pass123~"

RabbitMQ__HostName: "rabbitmq"

RabbitMQ__UserName: "ruser"

RabbitMQ__Password: "pass123~"

RabbitMQ__UserSubscriptionQueue__Queue: "orderservice.user.created"

RabbitMQ__UserSubscriptionQueue__ExchangeName: "UserExchange"

RabbitMQ__UserSubscriptionQueue__RoutingKey: "user.created"

RabbitMQ__OrderPublishQueue__Queue: "orderservice.order.created"

RabbitMQ__OrderPublishQueue__ExchangeName: "OrderExchange"

RabbitMQ__OrderPublishQueue__RoutingKey: "order.created"

  

notificationserviceapi:

build:

context: .

dockerfile: NotificationService.Api/Dockerfile

ports:

- "5003:8080"

depends_on:

rabbitmq:

condition: service_healthy

db:

condition: service_started

environment:

ConnectionStrings__DefaultConnection: "Host=db;Database=event-driven-notification-service;Username=postgres;Password=pass123~"

RabbitMQ__HostName: "rabbitmq"

RabbitMQ__UserName: "ruser"

RabbitMQ__Password: "pass123~"

# Subscription for UserCreated events

RabbitMQ__UserSubscriptionQueue__Queue: "notificationservice.user.created"

RabbitMQ__UserSubscriptionQueue__ExchangeName: "UserExchange"

RabbitMQ__UserSubscriptionQueue__RoutingKey: "user.created"

# Subscription for OrderCreated events

RabbitMQ__OrderSubscriptionQueue__Queue: "notificationservice.order.created"

RabbitMQ__OrderSubscriptionQueue__ExchangeName: "OrderExchange"

RabbitMQ__OrderSubscriptionQueue__RoutingKey: "order.created"

  

# SMTP Configuration

Smtp__Host: "smtp4dev"

Smtp__Port: 25

Smtp__FromAddress: "emailsender@domain.com"

```

  

## Running the Application

  

To run the application, use the following command:

  

```sh

docker-compose  up  --build

```

  

This will start all the services defined in the `docker-compose.yml` file.

  

## Conclusion

  

This project demonstrates how to build a scalable, event-driven architecture using .NET 8 and Docker. The microservices communicate asynchronously using RabbitMQ, allowing each service to operate independently while still being part of a cohesive system. The `NotificationService` enhances user engagement by sending welcome emails upon user registration and order confirmation emails when an order is placed.

  

Feel free to explore, modify, and expand upon this example to suit your own needs!