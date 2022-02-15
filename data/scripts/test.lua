px = 0
py = 0

function main()
	lh:StartMinigame("test")
end

function update() 
	lh:Draw(px, py, "@", 11) 
end

function input(ch)
	if ch == "a" then
		px = px - 1 
	elseif ch == "d" then
		px = px + 1
	elseif ch == "w" then
		py = py - 1
	elseif ch == "s" then
		py = py + 1
	end
end