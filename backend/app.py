from flask import Flask, jsonify, request

from langchain.messages import AIMessage
from langchain.tools import tool
from langchain_google_genai import ChatGoogleGenerativeAI

import dotenv

from painter import painter_llm
from prompts import mixer_prompt, painter_prompt
from mixer import mixer_llm

dotenv.load_dotenv()

app = Flask(__name__)


@app.route("/mixer")
def run_mixer():
    target_colors = request.args.get("color")
    owned_colors = request.args.get("owned").split(" ")
    available_colors = request.args.get("available").split(" ")

    mixer_owned_red_paint = int(owned_colors[0])
    mixer_owned_green_paint = int(owned_colors[1])
    mixer_owned_blue_paint = int(owned_colors[2])

    print(", ".join(target_colors.split(" ")))
    messages = [("system", mixer_prompt),
                ("human",
                 f"You need to obtain {", ".join(target_colors.split(" "))}. You have {mixer_owned_red_paint} red paint, {mixer_owned_green_paint} green paint, and {mixer_owned_blue_paint} blue paint. Currently, the following colors are already available to you without mixing: {int(available_colors[0])} red, {int(available_colors[1])} green, {int(available_colors[2])} blue, {int(available_colors[3])} yellow, {int(available_colors[4])} magenta, and {int(available_colors[5])} cyan."),
                ]

    result = mixer_llm.invoke(messages)

    if isinstance(result, AIMessage) and result.tool_calls:
        print(result.content)
        print(result.tool_calls)
        print(result.usage_metadata)
        print(result)
    return jsonify(
        result=result.tool_calls,
    )


@app.route("/painter")
def run_painter():
    template = request.args.get("template")
    owned_colors = request.args.get("owned").split(" ")
    available_colors = request.args.get("available").split(" ")

    painter_owned_red_paint = int(owned_colors[0])
    painter_owned_green_paint = int(owned_colors[1])
    painter_owned_blue_paint = int(owned_colors[2])
    painter_owned_yellow_paint = int(owned_colors[3])
    painter_owned_cyan_paint = int(owned_colors[4])
    painter_owned_magenta_paint = int(owned_colors[5])

    print(template)

    messages = [("system", painter_prompt),
                ("human",
                 f"You need to fill in this template: {template}. You have {painter_owned_red_paint} red paint, {painter_owned_green_paint} green paint, {painter_owned_blue_paint} blue paint, {painter_owned_yellow_paint} yellow paint, {painter_owned_magenta_paint} magenta paint, and {painter_owned_cyan_paint} cyan paint. Currently, the following colors are already available to you without mixing: {int(available_colors[0])} red, {int(available_colors[1])} green, {int(available_colors[2])} blue, {int(available_colors[3])} yellow, {int(available_colors[4])} magenta, and {int(available_colors[5])} cyan."),
                ]

    result = painter_llm.invoke(messages)

    if isinstance(result, AIMessage) and result.tool_calls:
        print(result.content)
        print(result.tool_calls)
        print(result.usage_metadata)
        print(result)
    return jsonify(
        result=result.tool_calls,
    )
