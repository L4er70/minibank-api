# Stage 1: Build (The "Kitchen")
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

# Stage 2: Serve (The "Counter")
FROM nginx:alpine
# Copy the compiled 'build' folder to Nginx's public directory
COPY --from=build /app/build /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]