# Real-Time Analytics Platform

![.NET CI](https://github.com/heidari2020/RealTimeAnalyticsPlatform/actions/workflows/dotnet-ci.yml/badge.svg) *(جایگزین لینک بج خودت کن)*[![.NET CI Pipeline](https://github.com/heidari2020/RealTimeAnalyticsPlatform/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/heidari2020/RealTimeAnalyticsPlatform/actions/workflows/dotnet-ci.yml)

A distributed, event-driven platform built with **.NET 8** that simulates vehicle GPS data, processes it in real-time via **Apache Kafka**, caches the latest locations in **Redis**, flags speed violations in **MongoDB**, and exposes the data through a **Minimal API**.

## 🏗️ Architecture

The system follows an event-driven microservices architecture. The flow of data is as follows:

```mermaid
graph TD
    A[VehicleSimulator<br/>Producer] -->|JSON via Kafka| B((Kafka Topic:<br/>vehicle-locations))
    B --> C[VehicleConsumer<br/>Consumer]
    C -->|Latest Location| D[(Redis<br/>Cache)]
    C -->|Speed Alerts > 100km/h| E[(MongoDB<br/>Database)]
    F[VehicleLocationApi<br/>Minimal API] -->|Read Location| D
    F -->|Read Alerts| E
    G((Client / Swagger)) -->|HTTP GET| F

## 🚀 Features

- Apache Kafka for event streaming (KRaft mode)
- Kafdrop UI for monitoring topics
- Vehicle location simulator (Producer) with .NET 8
- Dockerized: The entire infrastructure and application stack runs with a single docker-compose command.
- CI/CD: Automated build pipeline using GitHub Actions.
- Redis: Used as a fast cache for the latest vehicle locations (Key-Value).
- MongoDB: Used as a NoSQL document store for historical speed alerts.

## 🛠️ Technologies

- .NET 8
- Apache Kafka 7.5 (KRaft)
- Kafdrop
- Docker & Docker Compose
- Databases: Redis (Cache), MongoDB (NoSQL)
- DevOps: Docker, Docker Compose, GitHub Actions

## 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

## 🔧 Getting Started

### 1. Start Kafka and Kafdrop

```bash
docker-compose up -d# RealTimeAnalyticsPlatform