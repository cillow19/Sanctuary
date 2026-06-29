import express from "express";
import bcrypt from "bcryptjs";
import { randomUUID } from "crypto";

const router = express.Router();

// Inject db via middleware from server.js
let db = null;
export function setDatabase(database) {
  db = database;
}

// Helper to generate UUID
function generateSessionId() {
  return randomUUID().replace(/-/g, "");
}

// Validate payload
function validateLoginRequest(data) {
  const errors = {};
  if (!data.username || typeof data.username !== "string") {
    errors.username = "Username is required";
  }
  if (!data.password || typeof data.password !== "string") {
    errors.password = "Password is required";
  }
  return Object.keys(errors).length > 0 ? errors : null;
}

function validateRegisterRequest(data) {
  const errors = {};
  if (!data.username || typeof data.username !== "string") {
    errors.username = "Username is required";
  }
  if (!data.password || typeof data.password !== "string") {
    errors.password = "Password is required";
  }
  if (data.password != data.confirmPassword) {
    errors.confirmPassword = "Passwords do not match";
  }
  return Object.keys(errors).length > 0 ? errors : null;
}

// POST /login
router.post("/login", (req, res) => {
  const errors = validateLoginRequest(req.body);
  if (errors) {
    return res.status(400).json({ errors });
  }

  const { username, password } = req.body;

  db.get(
    "SELECT Id, Username, Password FROM Users WHERE Username = ?",
    [username],
    async (err, dbUser) => {
      if (err) {
        console.error("DB error:", err);
        return res.status(500).json({ error: "Database error" });
      }

      if (!dbUser) {
        console.warn(`Login failed, user not found for username: ${username}`);
        return res.status(401).json({ error: "Invalid credentials" });
      }

      // Verify password with BCrypt
      try {
        const isPasswordValid = await bcrypt.compare(password, dbUser.Password);
        if (!isPasswordValid) {
          console.warn(`Login failed, invalid password for username: ${username}`);
          return res.status(401).json({ error: "Invalid credentials" });
        }
      } catch (bcryptErr) {
        console.error("BCrypt error:", bcryptErr);
        return res.status(500).json({ error: "Authentication error" });
      }

      // Generate session
      const sessionId = generateSessionId();
      const now = new Date().toISOString();

      db.run(
        "UPDATE Users SET Session = ?, SessionCreated = ? WHERE Id = ?",
        [sessionId, now, dbUser.Id],
        (updateErr) => {
          if (updateErr) {
            console.error("Failed to update session info for username:", username, updateErr);
            return res.status(500).json({ error: "Internal server error" });
          }

          return res.json({
            sessionId,
            launchArguments: null
          });
        }
      );
    }
  );
});

// POST /register
router.post("/register", (req, res) => {
  const errors = validateRegisterRequest(req.body);
  if (errors) {
    return res.status(400).json({ errors });
  }

  const { username, password } = req.body;

  // Check if username already taken
  db.get(
    "SELECT Id FROM Users WHERE Username = ?",
    [username],
    async (err, existingUser) => {
      if (err) {
        console.error("DB error:", err);
        return res.status(500).json({ error: "Database error" });
      }

      if (existingUser) {
        console.warn(`Registration failed, username already taken: ${username}`);
        return res.status(409).json({ error: "Username already taken" });
      }

      // Hash password with BCrypt
      try {
        const salt = await bcrypt.genSalt(10);
        const hashedPassword = await bcrypt.hash(password, salt);

        db.run(
          "INSERT INTO Users (Username, Password) VALUES (?, ?)",
          [username, hashedPassword],
          (insertErr) => {
            if (insertErr) {
              console.error("Failed to add new user:", username, insertErr);
              return res.status(500).json({ error: "Internal server error" });
            }

            return res.json({ success: true });
          }
        );
      } catch (bcryptErr) {
        console.error("BCrypt error:", bcryptErr);
        return res.status(500).json({ error: "Internal server error" });
      }
    }
  );
});

export default router;