module.exports = {
  "/authapi": {
    target:
      process.env["services__authapi__https__0"] ||
      process.env["services__authapi__http__0"],
    secure: process.env["NODE_ENV"] !== "development",
    pathRewrite: {
      "^/authapi": "",
    },
  },
};
