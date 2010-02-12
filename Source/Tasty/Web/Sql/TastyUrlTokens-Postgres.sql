DROP TABLE IF EXISTS "tasty_url_token" CASCADE;
CREATE TABLE "tasty_url_token"
(
	"key" character varying(64) NOT NULL,
	"type" character varying(512) NOT NULL,
	"data" text NOT NULL,
	"created" timestamp without time zone NOT NULL,
	"expires" timestamp without time zone NOT NULL,
	CONSTRAINT "tasty_url_token_pkey" PRIMARY KEY("key")
)
WITHOUT OIDS;
ALTER TABLE "tasty_url_token" OWNER TO tasty;

CREATE INDEX "tasty_url_token_expires" ON "tasty_url_token"("expires");