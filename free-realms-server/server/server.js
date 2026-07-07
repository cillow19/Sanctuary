import express from "express";
import cors from "cors";
import sqlite3pkg from "sqlite3";
import path from "path";
import { fileURLToPath } from "url";

import routes from "../routes/routes.js";
import authEndpoints, { setDatabase } from "../routes/authEndpoints.js";

const app = express();
const port = 20260;

// Fix for __dirname in ES modules
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// SQLite init
const sqlite3 = sqlite3pkg.verbose();
const dbFile = process.env.DB_FILE || path.join(__dirname, "..", "Sanctuary", "src", ".local_data", "sanctuary.db");
const db = new sqlite3.Database(dbFile, (err) => {
  if (err) console.error("Failed to open database:", dbFile, err);
  else console.log("Connected to DB at:", dbFile);
});

// Set database for auth endpoints
setDatabase(db);

const corsOptions = {
  origin: 'https://osfr-cats-server.dev', // Allow requests from this origin
  optionsSuccessStatus: 200 // Some legacy browsers (IE11, various mobile) choke on 204
};

app.use(cors(corsOptions));
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Health check
app.get("/health", (req, res) => {
    res.send("Hello, World!");
});

// Routes
app.use("/", routes);


app.listen(port, () => {
    console.log(`Auth server running at port ${port}`);
});