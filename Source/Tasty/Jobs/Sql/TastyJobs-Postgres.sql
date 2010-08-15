DROP TABLE IF EXISTS "tasty_job" CASCADE;
CREATE TABLE "tasty_job"
(
	"id" serial NOT NULL,
	"name" character varying(128) NOT NULL,
	"type" character varying(512) NOT NULL,
	"data" text NOT NULL,
	"status" character varying(12) NOT NULL,
	"exception" text NULL,
	"queue_date" timestamp without time zone NOT NULL,
	"start_date" timestamp without time zone NULL,
	"finish_date" timestamp without time zone NULL,
	"schedule_name" character varying(128) NULL,
	CONSTRAINT "tasty_job_pkey" PRIMARY KEY("id")
)
WITHOUT OIDS;
ALTER TABLE "tasty_job" OWNER TO tasty;