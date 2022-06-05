const polka = require("polka");
const ping = require("ping");

const app = polka();

const ONE_MINUTE = 60 * 1000;
const serverCache = {};

app.get("/server/:ip", async (req, res) => {
  const { ip } = req.params;

  if (serverCache?.[ip]) {
    const lastPing = serverCache?.[ip];
    if (new Date() - new Date(lastPing?.date) < ONE_MINUTE) {
      console.log("serving status from cache");
      res.end(lastPing?.status ?? "Error");
    }
  }

  const result = await ping.promise.probe(ip, {
    timeout: 10,
    extra: ["-i", "2"],
  });

  const status = result.alive ? "Online" : "Offline";

  serverCache[ip] = {
    status: status,
    date: new Date(),
  };

  res.end(status);
});

const port = process.env["PORT"] || 3000;
app.listen(port, () => {
  console.log(`> Running on localhost:${port}`);
});
